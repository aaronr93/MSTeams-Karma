using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using NLog;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Azure.Documents.Client;
using MSTeams.Karma.BusinessLogic;
using MSTeams.Karma.Models;
using MSTeams.Karma.Properties;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.Controllers
{
    [BotAuthentication(MicrosoftAppIdSettingName = "MicrosoftAppId", MicrosoftAppPasswordSettingName = "MicrosoftAppPassword")]
    public class MessagesController : ApiController
    {
        public MessagesController()
        {
            _karmaLogic = new KarmaLogic();
        }

        private readonly KarmaLogic _karmaLogic;
        
        [HttpGet]
        [Route("healthcheck")]
        public async Task<string> HealthCheck()
        {
            string authKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
            string endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];
            DocumentClient client = DocumentDBRepository<KarmaModel>.GetDocumentClient(endpoint, authKey);
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

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    var messageResponse = await HandleActivityTypeMessage(activity, cancellationToken);
                    if (messageResponse != null)
                    {
                        response = messageResponse;
                    }
                    break;
            }

            return response;
        }

        private async Task<HttpResponseMessage> HandleActivityTypeMessage(Activity activity, CancellationToken cancellationToken)
        {
            switch (activity.Conversation.ConversationType)
            {
                case "personal":
                    return await HandlePersonalMessage(activity, cancellationToken);
                default:
                    return await HandleGroupMessage(activity, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> HandlePersonalMessage(Activity activity, CancellationToken cancellationToken)
        {
            // Check for forbidden commands.
            if (KarmaLogic.SomeoneWasGivenKarma(activity.Text) != null)
            {
                var reply = activity.CreateReply("You cannot change karma in a personal chat.", activity.Locale);
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                }

                Trace.TraceInformation($"<message>{activity.Text}</message><reply>{reply.Text}</reply>");
            }

            // Add things you *can* do in personal chat (like leaderboard checking) below.
            return await ShowHelpMessage(activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> HandleGroupMessage(Activity activity, CancellationToken cancellationToken)
        {
            // Check for commands.
            if (KarmaLogic.SomeoneWasGivenKarma(activity.Text) != null)
            {
                // Karma command
                return await HandleKarmaChange(activity, cancellationToken);
            }

            // Add more commands here.
            return await ShowHelpMessage(activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> ShowHelpMessage(Activity activity, CancellationToken cancellationToken)
        {
            var replyBuilder = new StringBuilder();
            replyBuilder.AppendLine("To change karma, mention me (@karma), then add between 2 and 6 pluses or minuses to add or remove karma.");
            replyBuilder.AppendLine("Examples:");
            replyBuilder.AppendLine("    @karma msteams-- (removes 1 karma)");
            replyBuilder.AppendLine("    msteams-- @karma (removes 1 karma) (you can mention me anywhere in the message)");
            replyBuilder.AppendLine("    @karma 🤔 ++++++ (adds 5 karma)");
            replyBuilder.AppendLine($"    @karma \"Alex's Confluence pages\"+++++++++++ (adds 5 karma; limit enforced by {Strings.BuzzkillMode}");
            replyBuilder.AppendLine($"More information: {Strings.MoreInformationLink}");

            var reply = activity.CreateReply(replyBuilder.ToString(), activity.Locale);
            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
            }

            return null;
        }

        private async Task<HttpResponseMessage> HandleKarmaChange(Activity activity, CancellationToken cancellationToken)
        {
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

            // Strip the mention of this bot
            Utilities.RemoveBotMentionsFromActivityText(activity);

            // Process the alleged Karma instruction and add the response message
            var replyMessage = await _karmaLogic.GetReplyMessageForKarma(activity.Text);
            if (string.IsNullOrEmpty(replyMessage))
            {
                return null;
            }

            reply.Text += replyMessage;

            // Send the response. We need a new ConnectorClient each time so that this action is thread-safe.
            // For example, multiple teams may call the bot simultaneously; it should respond to the right conversation.
            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
            }

            Trace.TraceInformation($"<message>{activity.Text}</message><reply>{reply.Text}</reply>");

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}