using System.Net;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MSTeams.Karma.Util;
using NLog;
using System.Configuration;
using Microsoft.Azure.Documents.Client;
using MSTeams.Karma.Models;

namespace MSTeams.Karma.Controllers
{
    /// <summary>
    /// Main messaging controller.
    /// </summary>
    /// <seealso cref="ApiController" />
    [BotAuthentication(CredentialProviderType = typeof(CustomCredsProvider))]
    [TenantFilter]
    public class MessagesController : ApiController
    {
        private static readonly string AuthKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
        private static readonly string Endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];
        private static ILogger Logger => LogManager.GetCurrentClassLogger();
        
        [HttpGet]
        [Route("healthcheck")]
        public async Task<string> HealthCheck()
        {
            DocumentClient client = DocumentDBRepository<KarmaModel>.GetDocumentClient(Endpoint, AuthKey);
            string responseMessage;

            try
            {
                await client.OpenAsync();
                responseMessage = "Successfully opened database.";
            }
            catch (Exception)
            {
                responseMessage = "Unable to open database.";
            }
            finally
            {
                client.Dispose();
            }

            return responseMessage;
        }

        [HttpGet]
        public string Get()
        {
            return "GET not supported. Try /healthcheck or /post, the latter within MS Teams.";
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Type != ActivityTypes.Message)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            // Strip the mention of this bot
            Utilities.RemoveBotMentionsFromActivityText(activity);

            // Compose the bot's response
            var reply = activity.CreateReply(string.Empty, activity.Locale);

            // Get Mentions
            //var userMentions = Utilities.GetUserMentions(activity).ToList();
            //if (userMentions.Any())
            //{
            //    // Only support 1 karma at a time, for now.
            //    var firstMention = userMentions.First();
            //    // Mention the user that was given karma
            //    if (KarmaLogic.MentionedUserWasGivenKarma(activity.Text, firstMention.Text))
            //    {
            //        //reply.AddMentionToText(firstMention.Mentioned);
            //    }
            //}

            // Process the alleged Karma instruction and add the response message
            var replyMessage = await KarmaLogic.GetReplyMessageForKarma(activity.Text);
            if (string.IsNullOrEmpty(replyMessage))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            reply.Text += replyMessage;

            // Send the response. We need a new ConnectorClient each time so that this action is thread-safe.
            // For example, multiple teams may call the bot simultaneously; it should respond to the right conversation.
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);

            Logger.Info($"<message>{activity.Text}</message><reply>{reply.Text}</reply>");

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}