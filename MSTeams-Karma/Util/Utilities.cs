using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Connector;

namespace MSTeams.Karma
{
    public class Utilities
    {
        public const string EntityRegex = @"((?:[^-+\""\s]+?)|(?:\""[^-+\""]+?\"")|(?:<at>[^-+\""]+?<\/at>))";
        public const string WhitespaceRegex = @"[\s]+";

        public static IEnumerable<Mention> GetUserMentions(Activity activity)
        {
            return activity.GetMentions()
                .ToList()
                .Where(a => a.Mentioned.Id != activity.Recipient.Id)
                .ToList();
        }

        public static void RemoveBotMentionsFromActivityText(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            foreach (var m in activity.GetMentions())
            {
                if (m.Mentioned.Id == activity.Recipient.Id)
                {
                    //Bot is in the @mention list.  
                    //The below example will strip the bot name out of the message, so you can parse it as if it wasn't included.
                    //Note that the Text object will contain the full bot name, if applicable.
                    if (m.Text != null)
                    {
                        activity.Text = activity.Text.Replace(m.Text, "").Trim();
                    }
                }
            }
        }

        public static string TrimWhitespace(string text)
        {
            if (text == null)
            {
                return null;
            }
            return text.Trim().Trim('\n', '\r', ' ').Replace("\\r", "").Replace("\\n", "");
        }

    }
}