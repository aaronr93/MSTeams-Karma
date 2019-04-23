using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using MSTeams.Karma.Models;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsScoreLogic
    {
        public TeamsScoreLogic(IDocumentDbRepository<KarmaModel> db)
        {
            _db = db;
        }

        private readonly IDocumentDbRepository<KarmaModel> _db;

        public async Task<string> GetScore(Activity activity, Match match, CancellationToken cancellationToken)
        {
            KarmaModel karma;
            string response = null;
            var mention = Utilities.GetUserMentions(activity).FirstOrDefault();
            var entity = match.Groups[1].Value;

            if (mention != null)
            {
                karma = await _db.GetItemAsync(mention.Mentioned.Id, "msteams_user", cancellationToken);
                response = $"{mention.Mentioned.Name}'s karma is {karma.Score}";
            }
            else if (!string.IsNullOrEmpty(entity))
            {
                karma = await _db.GetItemAsync(entity.ToLower(), "msteams_entity", cancellationToken);
                response = $"{entity}'s karma is {karma.Score}";
            }
            
            return response;
        }
    }
}