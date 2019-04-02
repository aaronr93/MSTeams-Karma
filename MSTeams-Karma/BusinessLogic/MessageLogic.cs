using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MSTeams.Karma.BusinessLogic
{
    public class MessageLogic
    {
        public bool IsAskingForHelp(string message)
        {
            return Regex.IsMatch(message, "(help|-h|idk|\\?|man )");
        }
    }
}