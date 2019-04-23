using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsKarmaLogic
    {
        private readonly KarmaLogic _karmaLogic;
        private const string MultiWordKarmaRegexPattern = "\".*?\"[ ]?(\\+{2,6}|-{2,6})";

        public TeamsKarmaLogic(KarmaLogic karmaLogic)
        {
            _karmaLogic = karmaLogic;
        }

        public async Task<IList<string>> GetKarmaResponseTextsAsync(Activity activity, CancellationToken cancellationToken)
        {
            List<string> responses = new List<string>();

            // Get Mentions
            var userMentions = Utilities.GetUserMentions(activity).ToList();

            // Remove the space before the karma score
            activity.Text = Regex.Replace(activity.Text, "[ ]([+-]{2,})", "$1");

            // Remove spaces in entity names
            foreach (var mention in userMentions)
            {
                var mentionName = mention.Mentioned.Name;
                var spaceStrippedMention = mentionName.Replace(" ", "");
                activity.Text = activity.Text.Replace(mentionName, spaceStrippedMention);
            }

            var karmaChanges = new List<KarmaChange>();
            var multiWordKarmaMatches = Regex.Matches(activity.Text, MultiWordKarmaRegexPattern, RegexOptions.RightToLeft);
            foreach (Match match in multiWordKarmaMatches)
            {
                var karmaStr = match.Value;
                var cleanName = karmaStr.TrimEnd(' ', '+', '-').Trim('\"');
                karmaChanges.Add(new KarmaChange(karmaStr, cleanName, cleanName.ToLower()));
                // less frustrating to have "test karma" and "Test Karma" use the same karma.
                activity.Text = activity.Text.Replace(karmaStr, "");
            }

            // Get list of karma strings
            var separatedBySpaces = activity.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add all the user karma strings
            foreach (var mention in userMentions)
            {
                string mentionName = mention.Mentioned.Name;
                string spaceStrippedMention = mentionName.Replace(" ", "");
                string karmaString = separatedBySpaces.FirstOrDefault(a => a.Contains(spaceStrippedMention));
                separatedBySpaces.Remove(karmaString);
                karmaChanges.Add(new KarmaChange(karmaString, mention.Mentioned.Name, mention.Mentioned.Id));
            }

            // Now add all the non-user karma strings
            foreach (var nonUserKarmaString in separatedBySpaces)
            {
                string cleanName = nonUserKarmaString.TrimEnd(' ', '+', '-').Trim('\"');
                karmaChanges.Add(new KarmaChange(nonUserKarmaString, cleanName, cleanName.ToLower()));
            }

            // Generate messages
            foreach (var karmaChange in karmaChanges.Distinct(new DuplicateKarmaComparer()))
            {
                // Process the alleged Karma instruction and add the response message
                if (KarmaLogic.IsKarmaString(karmaChange.KarmaString))
                {
                    var replyMessage = await _karmaLogic.GetReplyMessageForKarma(karmaChange.KarmaString, karmaChange.UniqueId, karmaChange.Name, cancellationToken);
                    if (string.IsNullOrEmpty(replyMessage))
                    {
                        return null;
                    }

                    responses.Add(replyMessage);
                }
            }
            
            // Add space back... "AaronRosenberger" -> "Aaron Rosenberger"
            foreach (var mention in userMentions)
            {
                var mentionName = mention.Mentioned.Name;
                var spaceStrippedMention = mentionName.Replace(" ", "");
                responses = responses.Select(a => a = a.Replace(spaceStrippedMention, mentionName)).ToList();
            }

            // Remove extra line breaks
            responses = responses.Select(Utilities.TrimWhitespace).ToList();

            return responses;
        }

        private class KarmaChange
        {
            public KarmaChange(string karmaString, string name, string uniqueId)
            {
                KarmaString = karmaString ?? throw new ArgumentNullException(nameof(karmaString));
                Name = name ?? throw new ArgumentNullException(nameof(name));
                UniqueId = uniqueId ?? throw new ArgumentNullException(nameof(uniqueId));
            }

            public string KarmaString { get; }
            public string Name { get; }
            public string UniqueId { get; }

            public override bool Equals(object obj)
            {
                if (!(obj is KarmaChange other))
                {
                    return false;
                }

                // We don't care if the Name is different, because it could be "Aaron" or "Aaron Rosenberger"...same person.
                return KarmaString.Equals(other.KarmaString) && UniqueId.Equals(other.UniqueId);
            }

            public override int GetHashCode()
            {
                return KarmaString.GetHashCode() * 17 + UniqueId.GetHashCode();
            }
        }

        private class DuplicateKarmaComparer : IEqualityComparer<KarmaChange>
        {
            public bool Equals(KarmaChange x, KarmaChange y)
            {
                return x.UniqueId.Equals(y.UniqueId);
            }

            public int GetHashCode(KarmaChange obj)
            {
                return obj.UniqueId.GetHashCode();
            }
        }
    }
}