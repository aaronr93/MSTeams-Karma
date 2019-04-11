using System;
using System.Collections.Generic;
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
            _messageLogic = new MessageLogic();
            _teamsKarmaLogic = new TeamsKarmaLogic();
            _teamsToggleLogic = new TeamsToggleLogic();
        }
        
        private readonly MessageLogic _messageLogic;
        private readonly TeamsKarmaLogic _teamsKarmaLogic;
        private readonly TeamsToggleLogic _teamsToggleLogic;
        
        [HttpGet]
        [Route("healthcheck")]
        public async Task<string> HealthCheck()
        {
            string authKey = ConfigurationManager.AppSettings["AzureCosmosPrimaryAuthKey"];
            string endpoint = ConfigurationManager.AppSettings["AzureCosmosEndpoint"];
            DocumentClient client = DocumentDBRepository<KarmaModel>.Default.GetDocumentClient(endpoint, authKey);
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
            activity.Text = Utilities.TrimWhitespace(activity.Text);
            // Check for forbidden commands.
            if (KarmaLogic.SomeoneReceivedKarmaInWholeMessage(activity.Text))
            {
                var reply = activity.CreateReply("You cannot change karma in a personal chat.", activity.Locale);
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                }

                Trace.TraceInformation($"<message>{activity.Text}</message><reply>{reply.Text}</reply>");
            }

            // Add things you *can* do in personal chat (like leaderboard checking) below.

            if (_messageLogic.IsAskingForHelp(activity.Text))
            {
                return await SendHelpMessage(activity, cancellationToken);
            }

            return await SendMessage(Strings.DidNotUnderstand, activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> HandleGroupMessage(Activity activity, CancellationToken cancellationToken)
        {
            activity.Text = Utilities.TrimWhitespace(activity.Text);
            // Check for commands.
            if (KarmaLogic.SomeoneReceivedKarmaInWholeMessage(activity.Text))
            {
                // Karma command
                return await HandleKarmaChange(activity, cancellationToken);
            }

            // Add more commands here.

            if (_messageLogic.IsAskingForHelp(activity.Text))
            {
                return await SendHelpMessage(activity, cancellationToken);
            }

            if (_messageLogic.IsDisablingBot(activity.Text))
            {
                return await _teamsToggleLogic.DisableBotInChannel(activity);
            }
            else if (_messageLogic.IsEnablingBot(activity.Text))
            {
                return await _teamsToggleLogic.EnableBotInChannel(activity);
            }

            return await SendMessage(Strings.DidNotUnderstand, activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendHelpMessage(Activity activity, CancellationToken cancellationToken)
        {
            return await SendMessage($"{Strings.SmallHelpMessage} More information: {Strings.MoreInformationLink}", activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendMessage(string text, Activity activity, CancellationToken cancellationToken)
        {
            var reply = activity.CreateReply(text, activity.Locale);
            
            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
            }

            return null;
        }

        private async Task<HttpResponseMessage> HandleKarmaChange(Activity activity, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"Message: {activity.Text}");

            // Compose the bot's response
            var reply = activity.CreateReply(string.Empty, activity.Locale);

            // Strip the mention of this bot
            Utilities.RemoveBotMentionsFromActivityText(activity);

            var replies = await _teamsKarmaLogic.GetKarmaResponseTextsAsync(activity);

            // If a lot of responses need to be given, put them all into one message.
            if (replies.Count > 5)
            {
                var sb = new StringBuilder();
                foreach (var replyMsg in replies)
                {
                    sb.AppendLine($"{replyMsg};\n");
                }
                reply.Text = sb.ToString();

                // Send the response. We need a new ConnectorClient each time so that this action is thread-safe.
                // For example, multiple teams may call the bot simultaneously; it should respond to the right conversation.
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                }

                Trace.TraceInformation($"Reply: {reply.Text}");
            }
            else
            {
                // Otherwise, 3 separate messages is fine, and preferable for readability and better engagement.
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    foreach (var replyMsg in replies)
                    {
                        reply.Text = replyMsg;
                        await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                        
                        Trace.TraceInformation($"Reply: {reply.Text}");
                    }
                }
            }


            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}