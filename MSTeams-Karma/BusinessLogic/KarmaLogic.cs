using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;
using NLog;

namespace MSTeams.Karma.BusinessLogic
{
    public class KarmaLogic
    {
        // To test the Regex: https://regexr.com/4aa2a
        private static readonly Regex KarmaRegex = new Regex($@"^{Utilities.EntityRegex}[ ]*([-]{{2,}}|[+]{{2,}})(?:\s|$)+",
            RegexOptions.Compiled);
        private const string ReplyMessageIncreasedFormat = "{0}'s karma has increased to {1}";
        private const string ReplyMessageDecreasedFormat = "{0}'s karma has decreased to {1}";

        private static readonly List<string> KarmaBlacklist = new List<string>
        {
            "c++",
        };

        public KarmaLogic(IDocumentDbRepository<KarmaModel> documentDbRepository)
        {
            _db = documentDbRepository;
        }

        private readonly IDocumentDbRepository<KarmaModel> _db;

        /// <summary>
        /// Returns if entity was given karma.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsKarmaString(string message)
        {
            return KarmaRegex.IsMatch(message ?? string.Empty);
        }

        /// <summary>
        /// Returns if any entity was given karma.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool SomeoneReceivedKarmaInWholeMessage(string message)
        {
            return Regex.IsMatch(message ?? string.Empty, "[+-]{2,}");
        }

        private static string GetPartitionForKey(string karmaString)
        {
            if (!string.IsNullOrEmpty(karmaString) && karmaString.Contains("<at>"))
            {
                // This was a Teams user. Separate into its own partition.
                return "msteams_user";
            }
            else
            {
                return "msteams_entity";
            }
        }


        /// <summary>
        /// Responds to a karma request. NOTE: Assumes <paramref name="karmaString"/> is a valid karma string.
        /// </summary>
        /// <param name="karmaString">A valid karma string.</param>
        /// <param name="uniqueId">If the entity given karma was a Teams user, the unique ID for that user</param>
        /// <param name="givenName">If the entity given karma was a Teams user, the Given Name for that user</param>
        /// <returns>The bot's response including the karma amount difference.</returns>
        public async Task<string> GetReplyMessageForKarma(string karmaString, string uniqueId, string givenName, CancellationToken cancellationToken)
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
            string partition = GetPartitionForKey(karmaString);

            KarmaModel karmaItem = await _db.GetItemAsync(id, partition, cancellationToken);

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

            if (deltaLength > 5)
            {
                // BUZZKILL MODE
                deltaLength = 5;
                replyMessage += $"{Strings.BuzzkillMode} {Strings.BuzzkillModeMessage} ... ";
            }
            else if (deltaLength < -5)
            {
                deltaLength = -5;
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
                await _db.UpdateItemAsync(id, karmaItem, partition, cancellationToken);
            }
            else
            {
                await _db.CreateItemAsync(karmaItem, partition, cancellationToken);
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