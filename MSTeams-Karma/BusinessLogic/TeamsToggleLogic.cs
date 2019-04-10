using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsToggleLogic
    {
        public TeamsToggleLogic(IDocumentDbRepository<TeamsChannelMetadata> db = null)
        {
            _db = db ?? DocumentDBRepository<TeamsChannelMetadata>.Default;
        }

        private readonly IDocumentDbRepository<TeamsChannelMetadata> _db;
        private static bool _isEnabled = true;

        public async Task<bool> IsEnabledInChannel(string channelId, string teamId)
        {
            // Don't make a DB call for every message when it's enabled.
            if (_isEnabled)
            {
                return true;
            }
            // If it's disabled, check the database as a backup.
            try
            {
                var metadata = await _db.GetItemAsync("teamsChannelMetadata", channelId, teamId);
                return metadata.IsEnabled;
            }
            catch (Exception)
            {
                Trace.TraceError($"Error getting metadata for channel {channelId} in team {teamId}.");
                return true;
            }
        }
        
        public async Task<HttpResponseMessage> DisableBotInChannel(Activity activity)
        {
            // First, check if the user sending the request is a Team Administrator.
            if (activity.From.Role != "user")
            {
                return null;
            }
            if (!activity.From.Name.Contains("TechOps"))
            {
                // THIS IS NOT THE BEST. I KNOW. I can't figure out how to get the list of admins for a channel.
                // (The Teams Bot documentation sucks for advanced users.)
                return null;
            }

            _isEnabled = false;

            return null;
        }

        public async Task<HttpResponseMessage> EnableBotInChannel(Activity activity)
        {
            

            _isEnabled = true;

            return null;
        }
    }

    public class TeamsChannelMetadata
    {
        public string Id { get; set; }
        public bool IsEnabled { get; set; }
    }
}