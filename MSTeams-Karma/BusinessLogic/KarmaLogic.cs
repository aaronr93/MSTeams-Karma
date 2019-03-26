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
        private const string KarmaRegex = @"((?:[^-+\s]+?)|(?:\""[^-+]+?\"")|(?:<at>[^-+]+?<\/at>))[ ]*([-]{2,}|[+]{2,})(?:\s|$)";
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
        /// Returns an entity that was given karma, if any was given.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string SomeoneWasGivenKarma(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return null;
            }

            var karmaMatch = GetKarmaRegexMatch(message);
            if (karmaMatch == null || !karmaMatch.Success)
            {
                return null;
            }

            if (karmaMatch.Groups.Count > 1)
            {
                return karmaMatch.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private static string GetPartitionForKey(string key)
        {
            if (Partitions.Contains(key))
            {
                return Partitions.FirstOrDefault(a => a == key);
            }

            return DocumentDBRepository<KarmaModel>.DefaultPartition;
        }

        public async Task<string> GetReplyMessageForKarma(string originalMessage)
        {
            var karmaMatch = GetKarmaRegexMatch(originalMessage);

            // Entity: the string to which karma is given.
            var entityForMessaging = SomeoneWasGivenKarma(originalMessage);
            if (entityForMessaging == null)
            {
                return null;
            }
            var uniqueEntity = Regex.Replace(entityForMessaging, "<.*?>|@|\"", "").ToLower();
            uniqueEntity = Regex.Replace(uniqueEntity, "[\\s]+", "");

            // Delta: the karma in pluses or minuses.
            var delta = karmaMatch.Groups[2].Value;

            // The amount of karma to give or take away.
            var deltaLength = delta.Length - 1;
            
            // We don't want commonly used karma strings to be interpreted as karma, like "C++"
            if (KarmaBlacklist.Contains((entityForMessaging + delta).Replace(" ", "")))
            {
                return null;
            }

            var partition = GetPartitionForKey(uniqueEntity);
            var karmaItem = await DocumentDBRepository<KarmaModel>.GetItemAsync(uniqueEntity, partition);

            bool existsInDb = karmaItem != null;
            if (!existsInDb)
            {
                karmaItem = new KarmaModel
                {
                    Id = uniqueEntity,
                    Partition = partition,
                    Score = 0
                };
            }

            string replyMessage = string.Empty;

            if (deltaLength > 5)
            {
                // BUZZKILL MODE
                deltaLength = 5;
                replyMessage += $"{Strings.BuzzkillMode} {Strings.BuzzkillModeMessage} ... ";
            }
            
            if (delta.Contains("+"))
            {
                karmaItem.Score += deltaLength;
                replyMessage += string.Format(ReplyMessageIncreasedFormat, entityForMessaging, karmaItem.Score);
            }
            else if (delta.Contains("-"))
            {
                karmaItem.Score -= deltaLength;
                replyMessage += string.Format(ReplyMessageDecreasedFormat, entityForMessaging, karmaItem.Score);
            }

            if (existsInDb)
            {
                await DocumentDBRepository<KarmaModel>.UpdateItemAsync(uniqueEntity, karmaItem, partition);
            }
            else
            {
                await DocumentDBRepository<KarmaModel>.CreateItemAsync(karmaItem, partition);
            }

            return replyMessage;
        }

        private static Match GetKarmaRegexMatch(string text)
        {
            var karmaMatch = Regex.Match(text, KarmaRegex);

            if (!karmaMatch.Success || karmaMatch.Groups.Count != 3)
            {
                return null;
            }

            return karmaMatch;
        }

        //private static async Task<ConcurrentDictionary<string, KarmaModel>> GetKarmaFromCosmos()
        //{
        //    var contents = await DocumentDBRepository<Models.KarmaModel>.GetItemsAsync(x => !string.IsNullOrEmpty(x.Entity) && x.Score.HasValue);
        //    var karmaStore = contents.ToList();
        //    var karma = new ConcurrentDictionary<string, KarmaModel>();

        //    foreach (var item in karmaStore)
        //    {
        //        karma.TryAdd(item.Entity, new KarmaModel { Score = item.Score.Value });
        //    }

        //    return karma;
        //}
        
        //private static async Task WriteKarmaToCosmos(ConcurrentDictionary<string, KarmaModel> karma)
        //{
        //    foreach (var item in karma)
        //    {
        //        await DocumentDBRepository<Models.KarmaModel>.UpdateItemAsync(item.Key, item.Value);
        //    }
        //}
    }
}