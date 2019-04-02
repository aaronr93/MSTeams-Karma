using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;
using NLog;

namespace MSTeams.Karma.BusinessLogic
{
    public class KarmaLogic
    {
        // To test the Regex: https://regexr.com/4aa2a
        private static readonly Regex KarmaRegex = new Regex(@"((?:[^-+\s]+?)|(?:\""[^-+]+?\"")|(?:<at>[^-+]+?<\/at>))[ ]*([-]{2,}|[+]{2,})(?:\s|$)",
            RegexOptions.Compiled);
        private const string ReplyMessageIncreasedFormat = "{0}'s karma has increased to {1}";
        private const string ReplyMessageDecreasedFormat = "{0}'s karma has decreased to {1}";

        private static readonly List<string> KarmaBlacklist = new List<string>
        {
            "c++",
        };

        // Add entities to this list that are given karma a lot.
        // This will give them a different partition and better bandwidth usage.
        // In the future, some basic learning could dynamically update this list.
        private static readonly HashSet<string> Partitions = new HashSet<string>
        {
            "msteams"
        };

        /// <summary>
        /// Returns if entity was given karma.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool SomeoneWasGivenKarma(string message)
        {
            return KarmaRegex.IsMatch(message ?? string.Empty);
        }

        private static string GetPartitionForKey(string key)
        {
            if (Partitions.Contains(key))
            {
                return Partitions.FirstOrDefault(a => a == key);
            }

            return DocumentDBRepository<KarmaModel>.DefaultPartition;
        }


        /// <summary>
        /// Responds to a karma request. NOTE: Assumes <paramref name="karmaString"/> is a valid karma string.
        /// </summary>
        /// <param name="karmaString">A valid karma string.</param>
        /// <param name="uniqueId">If the entity given karma was a Teams user, the unique ID for that user</param>
        /// <param name="givenName">If the entity given karma was a Teams user, the Given Name for that user</param>
        /// <returns>The bot's response including the karma amount difference.</returns>
        public async Task<string> GetReplyMessageForKarma(string karmaString, string uniqueId = null, string givenName = null)
        {
            // We don't want commonly used karma strings to be interpreted as karma, like "C++"
            if (KarmaBlacklist.Contains(karmaString.Replace(" ", "")))
            {
                return null;
            }

            // Break down the karma string into parts
            Match karmaMatch = GetKarmaRegexMatch(karmaString);

            // Get the entity that was given karma
            string entityForMessaging = karmaMatch.Groups[1].Value;
            string uniqueEntity = givenName ?? Regex.Replace(entityForMessaging, "<.*?>|@|\"|[\\s]", "").ToLower();

            // Delta: the karma in pluses or minuses.
            string delta = karmaMatch.Groups[2].Value;
            // The amount of karma to give or take away.
            int deltaLength = delta.Length - 1;
            if (delta.Contains("-"))
            {
                deltaLength *= -1;
            }

            string id = uniqueId ?? uniqueEntity;

            string partition = GetPartitionForKey(id);
            KarmaModel karmaItem = await DocumentDBRepository<KarmaModel>.GetItemAsync(id, partition);

            bool existsInDb = karmaItem != null;
            if (!existsInDb)
            {
                karmaItem = new KarmaModel
                {
                    Id = id,
                    Entity = uniqueEntity,
                    Partition = partition,
                    Score = 0
                };
            }

            string replyMessage = string.Empty;

            if (Math.Abs(deltaLength) > 5)
            {
                // BUZZKILL MODE
                deltaLength = 5;
                replyMessage += $"{Strings.BuzzkillMode} {Strings.BuzzkillModeMessage} ... ";
            }
            
            string messageFormat;
            if (deltaLength > 0)
            {
                messageFormat = ReplyMessageIncreasedFormat;
            }
            else
            {
                messageFormat = ReplyMessageDecreasedFormat;
            }

            karmaItem.Score += deltaLength;
            replyMessage += string.Format(messageFormat, entityForMessaging, karmaItem.Score);

            if (existsInDb)
            {
                await DocumentDBRepository<KarmaModel>.UpdateItemAsync(id, karmaItem, partition);
            }
            else
            {
                await DocumentDBRepository<KarmaModel>.CreateItemAsync(karmaItem, partition);
            }

            return replyMessage;
        }

        private static Match GetKarmaRegexMatch(string text)
        {
            var karmaMatch = KarmaRegex.Match(text);

            if (!karmaMatch.Success || karmaMatch.Groups.Count != 3)
            {
                return null;
            }

            return karmaMatch;
        }
    }
}