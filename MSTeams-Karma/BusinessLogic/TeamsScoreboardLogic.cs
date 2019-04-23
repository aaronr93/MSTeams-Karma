using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MSTeams.Karma.Models;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsScoreboardLogic
    {
        public TeamsScoreboardLogic(IDocumentDbRepository<ScoreboardModel> db)
        {
            _db = db;
        }

        private readonly IDocumentDbRepository<ScoreboardModel> _db;

        public async Task<string> GetScoreboard(Activity activity, Match match, CancellationToken cancellationToken)
        {
            var topOrBottom = match.Groups[1].Value;
            string partition = "defaultpartition";
            if (MessageLogic.ThingsSlopPhrases.Contains(match.Groups[3].Value))
            {
                partition = "msteams_entity";
            }
            else if (MessageLogic.UsersSlopPhrases.Contains(match.Groups[3].Value))
            {
                partition = "msteams_user";
            }

            ScoreboardModel response = await _db.ExecuteStoredProcedureAsync($"/{_db.GetCollectionUri()}/sprocs/scoreboard", partition, cancellationToken, (topOrBottom == "bottom").ToString().ToLower());

            var sb = new StringBuilder();

            foreach (KarmaModel karma in response.Feed)
            {
                sb.AppendFormat("{0}={1}, ", karma.Entity, karma.Score);
            }

            return sb.ToString().TrimEnd(',', ' ');
        }
    }
}