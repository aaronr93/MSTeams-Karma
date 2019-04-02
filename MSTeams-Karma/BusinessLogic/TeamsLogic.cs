using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace MSTeams.Karma.BusinessLogic
{
    public class TeamsLogic
    {
        

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
    }
}