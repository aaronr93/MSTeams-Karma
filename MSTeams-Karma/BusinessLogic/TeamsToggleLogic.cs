using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsToggleLogic
    {
        public TeamsToggleLogic(IDocumentDbRepository<TeamsChannelMetadataModel> db)
        {
            _db = db;
        }

        private readonly IDocumentDbRepository<TeamsChannelMetadataModel> _db;
        private static bool _isEnabled = true;

        public bool IsEnabledInChannel => _isEnabled;

        public bool IsDisablingBot(string message)
        {
            return message.Contains(Strings.DisableCommand);
        }

        public bool IsEnablingBot(string message)
        {
            return message.Contains(Strings.EnableCommand);
        }

        public async Task<bool> DisableBotInChannel(IActivity activity, CancellationToken cancellationToken)
        {
            return await ToggleBotInChannel(activity, false, cancellationToken);
        }

        public async Task<bool> EnableBotInChannel(IActivity activity, CancellationToken cancellationToken)
        {
            return await ToggleBotInChannel(activity, true, cancellationToken);
        }

        private async Task<bool> ToggleBotInChannel(IActivity activity, bool enabled, CancellationToken cancellationToken)
        {
            // First, check if the user sending the request is a Team Administrator.
            if (activity.From.Role == "bot")
            {
                return false;
            }
            // THIS IS NOT THE BEST. I KNOW. I can't figure out how to get the list of admins for a channel.
            // (The Teams Bot documentation sucks for advanced users.)
            // Only let me and TechOps toggle the bot on and off.
            if (activity.From.Name.Contains("TechOps") || activity.From.Name.Contains("Aaron Rosenberger"))
            {

                string channelId = (string)activity.ChannelData["teamsChannelId"];
                string teamId;

                try
                {
                    teamId = (string)activity.ChannelData["teamsTeamId"];
                }
                catch (Exception)
                {
                    Trace.TraceError("Could not get team id.");
                    teamId = "defaultpartition";
                }

                _isEnabled = enabled;
                TeamsChannelMetadataModel metadata = await _db.GetItemAsync(channelId, teamId, cancellationToken);

                bool existsInDb = metadata != null;
                if (!existsInDb)
                {
                    metadata = new TeamsChannelMetadataModel
                    {
                        Id = channelId,
                        TeamId = teamId
                    };
                }

                metadata.IsEnabled = _isEnabled;

                if (existsInDb)
                {
                    await _db.UpdateItemAsync(metadata.Id, metadata, metadata.TeamId, cancellationToken);
                }
                else
                {
                    await _db.CreateItemAsync(metadata, metadata.TeamId, cancellationToken);
                }

                return true;
            }

            return false;
        }
    }
}