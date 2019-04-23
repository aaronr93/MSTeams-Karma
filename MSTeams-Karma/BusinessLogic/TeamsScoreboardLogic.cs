using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsScoreboardLogic
    {
        public TeamsScoreboardLogic(IDocumentDbRepository<KarmaModel> db)
        {
            _db = db;
        }

        private readonly IDocumentDbRepository<KarmaModel> _db;

        
    }
}