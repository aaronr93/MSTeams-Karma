using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace MSTeams.Karma
{
    internal class KarmaScore
    {
        public int Value { get; set; }
    }

    public static class KarmaLogic
    {
        private static ILogger Logger => LogManager.GetLogger("karma");

        // To test the Regex: https://regexr.com/4aa2a
        private const string KarmaRegex = @"((?:[^-+\s]+?)|(?:\""[^-+]+?\"")|(?:<at>[^-+]+?<\/at>))[ ]*([-]{2,}|[+]{2,})(?:\s|$)";
        private const string KarmaFilePath = @"C:\Projects\Personal\MSTeams-Karma\MSTeams-Karma\karma.txt";
        private const string ReplyMessageIncreasedFormat = "{0}'s karma has increased to {1}";
        private const string ReplyMessageDecreasedFormat = "{0}'s karma has decreased to {1}";

        static KarmaLogic()
        {
            try
            {
                _karma = GetKarmaFromFile();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static volatile ConcurrentDictionary<string, KarmaScore> _karma;

        private static readonly List<string> KarmaBlacklist = new List<string>
        {
            "c++",
        };

        public static bool MentionedUserWasGivenKarma(string message, string mentionedUserText)
        {
            var karmaMatch = GetKarmaRegexMatch(message);
            if (karmaMatch == null)
            {
                return false;
            }

            var rawEntity = karmaMatch.Groups[1].Value;
            return rawEntity == mentionedUserText;
        }

        public static string GetReplyMessageForKarma(string originalMessage)
        {
            var karmaMatch = GetKarmaRegexMatch(originalMessage);
            if (karmaMatch == null)
            {
                return null;
            }

            // Entity: the string to which karma is given.
            var rawEntity = karmaMatch.Groups[1].Value;
            var entity = Regex.Replace(rawEntity, "<.*?>|@|\"", "").ToLower();

            // Delta: the karma in pluses or minuses.
            var delta = karmaMatch.Groups[2].Value;

            // The amount of karma to give or take away.
            var deltaLength = delta.Length - 1;
            
            // We don't want commonly used karma strings to be interpreted as karma, like "C++"
            if (KarmaBlacklist.Contains((rawEntity + delta).Replace(" ", "")))
            {
                return null;
            }

            string replyMessage = string.Empty;

            if (deltaLength > 5)
            {
                // BUZZKILL MODE
                deltaLength = 5;
                replyMessage += "Buzzkill Mode™ has enforced a maximum change of 5 points ... ";
            }

            var karmaItem = _karma.GetOrAdd(entity, _ => new KarmaScore());
            lock (karmaItem)
            {
                if (delta.Contains("+"))
                {
                    karmaItem.Value += deltaLength;
                    replyMessage += string.Format(ReplyMessageIncreasedFormat, rawEntity, karmaItem.Value);
                }
                else if (delta.Contains("-"))
                {
                    karmaItem.Value -= deltaLength;
                    replyMessage += string.Format(ReplyMessageDecreasedFormat, rawEntity, karmaItem.Value);
                }
            }

            WriteKarmaToFile(_karma);

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

        private static ConcurrentDictionary<string, KarmaScore> GetKarmaFromFile()
        {
            var contents = System.IO.File.ReadAllLines(KarmaFilePath).ToList();
            var karma = new ConcurrentDictionary<string, KarmaScore>();

            var dict = contents
                .Select(l => l.Split('='))
                .Where(a => a.Length == 2)
                .ToDictionary(
                    a => a[0],
                    a => int.Parse(a[1]));

            foreach (var item in dict)
            {
                karma.TryAdd(item.Key, new KarmaScore { Value = item.Value });
            }

            return karma;
        }

        // TODO: Should I add locking to this?
        private static void WriteKarmaToFile(ConcurrentDictionary<string, KarmaScore> karma)
        {
            var fileContents = string.Join(Environment.NewLine,
                karma
                    .Select(a => $"{a.Key}={a.Value.Value}")
                    .ToList());

            System.IO.File.WriteAllText(KarmaFilePath, fileContents);
        }
    }
}