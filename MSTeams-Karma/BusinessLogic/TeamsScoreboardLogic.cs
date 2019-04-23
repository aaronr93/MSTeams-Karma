using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsScoreboardLogic
    {
        public TeamsScoreboardLogic(IDocumentDbRepository<KarmaModel> db)
        {
            _db = db;
        }

        private readonly IDocumentDbRepository<KarmaModel> _db;

        public async Task<HttpResponseMessage> GetScore(Activity activity, CancellationToken cancellationToken)
        {
            return null;
        }

        public async Task<HttpResponseMessage> GetScoreboard(Activity activity, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}