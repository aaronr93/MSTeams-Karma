using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsLogic
    {
        private readonly KarmaLogic _karmaLogic;
        private const string MultiWordKarmaRegexPattern = "\".*?\"[ ]?(\\+{2,6}|-{2,6})";

        public TeamsLogic(KarmaLogic karmaLogic = null)
        {
            _karmaLogic = karmaLogic ?? new KarmaLogic();
        }

        public async Task<IList<ChannelAccount>> GetTeamsConversationMembersAsync(string serviceUrl, string conversationId)
        {
            try
            {
                using (var connectorClient = new ConnectorClient(new Uri(serviceUrl)))
                {
                    return await connectorClient.Conversations.GetConversationMembersAsync(conversationId);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }

            return null;
        }

        public async Task<IList<string>> GetKarmaResponseTextsAsync(Activity activity)
        {
            List<string> responses = new List<string>();

            // Get Mentions
            var userMentions = Utilities.GetUserMentions(activity).ToList();

            // Remove spaces in entity names
            foreach (var mention in userMentions)
            {
                var mentionName = mention.Mentioned.Name;
                var spaceStrippedMention = mentionName.Replace(" ", "");
                activity.Text = activity.Text.Replace(mentionName, spaceStrippedMention);
            }

            var karmaStuff = new List<KarmaStuff>();
            var multiWordKarmaMatches = Regex.Matches(activity.Text, MultiWordKarmaRegexPattern, RegexOptions.RightToLeft);
            foreach (Match match in multiWordKarmaMatches)
            {
                var karmaStr = match.Value;
                var cleanName = karmaStr.TrimEnd(' ', '+', '-').Trim('\"');
                karmaStuff.Add(new KarmaStuff
                {
                    Name = cleanName,
                    KarmaString = karmaStr,
                    UniqueId = cleanName.ToLower()  // less frustrating to have "test karma" and "Test Karma" use the same karma.
                });
                activity.Text = activity.Text.Replace(karmaStr, "");
            }

            // Get list of karma strings
            var separatedBySpaces = activity.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add all the user karma strings
            foreach (var mention in userMentions)
            {
                var mentionName = mention.Mentioned.Name;
                var spaceStrippedMention = mentionName.Replace(" ", "");
                var karmaString = separatedBySpaces.FirstOrDefault(a => a.Contains(spaceStrippedMention));
                separatedBySpaces.Remove(karmaString);
                karmaStuff.Add(new KarmaStuff
                {
                    KarmaString = karmaString,
                    Name = mention.Mentioned.Name,
                    UniqueId = mention.Mentioned.Id
                });
            }

            // Now add all the non-user karma strings
            foreach (var otherEntity in separatedBySpaces)
            {
                karmaStuff.Add(new KarmaStuff
                {
                    KarmaString = otherEntity
                });
            }

            // Generate messages
            foreach (var stuff in karmaStuff)
            {
                // Process the alleged Karma instruction and add the response message
                if (KarmaLogic.SomeoneWasGivenKarma(stuff.KarmaString))
                {
                    var replyMessage = await _karmaLogic.GetReplyMessageForKarma(stuff.KarmaString, stuff.UniqueId, stuff.Name);
                    if (string.IsNullOrEmpty(replyMessage))
                    {
                        return null;
                    }

                    responses.Add(replyMessage);
                }
            }

            return responses;
        }

        public class KarmaStuff
        {
            public string KarmaString { get; set; }
            public string Name { get; set; }
            public string UniqueId { get; set; }
        }
    }
}