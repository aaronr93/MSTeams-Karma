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

        private const string ThingsSlopPhrases = "things|thing|thinsg|thnigs|thigns|htings|thisng";
        private const string UsersSlopPhrases = "users|user|usrs|usres|suers|usr|suer|usre";
        public bool IsGettingScore(string message)
        {
            var whitespace = Utilities.WhitespaceRegex;
            message = Utilities.TrimWhitespace(message);
            return Regex.IsMatch(message, $@"<at>karma</at>{whitespace}get{whitespace}(top|bottom){whitespace}({ThingsSlopPhrases}|{UsersSlopPhrases})[\s\n]*(\.|\!)?$") ||
                Regex.IsMatch(message, $@"<at>karma</at>{whitespace}get{whitespace}{Utilities.EntityRegex}[\s\n]*(\.|\!)?$");
        }
    }
}