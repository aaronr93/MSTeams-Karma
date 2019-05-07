using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MSTeams.Karma.BusinessLogic
{
    public class MessageLogic
    {
        public bool IsAskingForHelp(string message)
        {
            return Regex.IsMatch(message, "(help|-h|idk|\\?|man )");
        }

        public const string ThingsSlopPhrases = "things|thing|thinsg|thnigs|thigns|htings|thisng";
        public const string UsersSlopPhrases = "users|user|usrs|usres|suers|usr|suer|usre";

        private static string ScoreRegex = $@"(<at>karma</at>{Utilities.WhitespaceRegex}|^)get{Utilities.WhitespaceRegex}{Utilities.EntityRegex}[\s\n]*(\.|\!)?$";
        public Match IsGettingScore(string message)
        {
            message = Utilities.TrimWhitespace(message);
            return Regex.Match(message, ScoreRegex);
        }

        private static string ScoreboardRegex = $@"(<at>karma</at>{Utilities.WhitespaceRegex}|^)get{Utilities.WhitespaceRegex}(top|bottom){Utilities.WhitespaceRegex}(?:({ThingsSlopPhrases})|({UsersSlopPhrases}))[\s\n]*(\.|\!)?$";
        public Match IsGettingScoreboard(string message)
        {
            message = Utilities.TrimWhitespace(message);
            return Regex.Match(message, ScoreboardRegex);
        }
    }
}