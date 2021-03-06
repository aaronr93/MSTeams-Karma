﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using MSTeams.Karma.BusinessLogic;
using MSTeams.Karma.Properties;
using Activity = Microsoft.Bot.Connector.Activity;

namespace MSTeams.Karma.Controllers
{
    [BotAuthentication(MicrosoftAppIdSettingName = "MicrosoftAppId", MicrosoftAppPasswordSettingName = "MicrosoftAppPassword")]
    public class MessagesController : ApiController
    {
        public MessagesController(MessageLogic messageLogic,
                                  Lazy<TeamsKarmaLogic> teamsKarmaLogic,
                                  Lazy<TeamsToggleLogic> teamsToggleLogic,
                                  Lazy<TeamsScoreboardLogic> teamsScoreboardLogic,
                                  Lazy<TeamsScoreLogic> teamsScoreLogic)
        {
            _messageLogic = messageLogic;
            _teamsKarmaLogic = teamsKarmaLogic;
            _teamsToggleLogic = teamsToggleLogic;
            _teamsScoreboardLogic = teamsScoreboardLogic;
            _teamsScoreLogic = teamsScoreLogic;
        }
        
        private readonly MessageLogic _messageLogic;
        private readonly Lazy<TeamsKarmaLogic> _teamsKarmaLogic;
        private readonly Lazy<TeamsToggleLogic> _teamsToggleLogic;
        private readonly Lazy<TeamsScoreboardLogic> _teamsScoreboardLogic;
        private readonly Lazy<TeamsScoreLogic> _teamsScoreLogic;

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

            var scoreboardRegexMatch = _messageLogic.IsGettingScoreboard(activity.Text);
            if (scoreboardRegexMatch.Success)
            {
                return await HandleScoreboardRequest(activity, scoreboardRegexMatch, cancellationToken);
            }
            else
            {
                var scoreRegexMatch = _messageLogic.IsGettingScore(activity.Text);
                if (scoreRegexMatch.Success)
                {
                    return await HandleScoreRequest(activity, scoreRegexMatch, cancellationToken);
                }
            }

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
            var enableDisableSucceeded = await CheckEnableDisable(activity, cancellationToken);
            if (enableDisableSucceeded)
            {
                return null;
            }

            if (!_teamsToggleLogic.Value.IsEnabledInChannel)
            {
                return null;
            }

            activity.Text = Utilities.TrimWhitespace(activity.Text);

            var scoreboardRegexMatch = _messageLogic.IsGettingScoreboard(activity.Text);
            if (scoreboardRegexMatch.Success)
            {
                return await HandleScoreboardRequest(activity, scoreboardRegexMatch, cancellationToken);
            }
            else
            {
                var scoreRegexMatch = _messageLogic.IsGettingScore(activity.Text);
                if (scoreRegexMatch.Success)
                {
                    return await HandleScoreRequest(activity, scoreRegexMatch, cancellationToken);
                }
            }

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

            // Often, users respond with a gif just after karma has been given. 
            // they also accidentally mention @karma. Don't respond to this.
            if (activity.Attachments.Any(a => a.ContentType.Contains("image")))
            {
                return null;
            }

            return await SendMessage(Strings.DidNotUnderstand, activity, cancellationToken);
        }

        private async Task<bool> CheckEnableDisable(Activity activity, CancellationToken cancellationToken)
        {
            bool success = false;
            string successMessage = null;

            if (_teamsToggleLogic.Value.IsDisablingBot(activity.Text))
            {
                success = await _teamsToggleLogic.Value.DisableBotInChannel(activity, cancellationToken);
                successMessage = Strings.DisableBotSuccess;
            }
            else if (_teamsToggleLogic.Value.IsEnablingBot(activity.Text))
            {
                success = await _teamsToggleLogic.Value.EnableBotInChannel(activity, cancellationToken);
                successMessage = Strings.EnableBotSuccess;
            }

            if (success)
            {
                var reply = activity.CreateReply(successMessage, activity.Locale);

                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);
                }
            }

            return success;
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

        private async Task<HttpResponseMessage> HandleScoreRequest(Activity activity, Match match, CancellationToken cancellationToken)
        {
            var response = await _teamsScoreLogic.Value.GetScore(activity, match, cancellationToken);
            return await SendMessage(response, activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> HandleScoreboardRequest(Activity activity, Match match, CancellationToken cancellationToken)
        {
            var response = await _teamsScoreboardLogic.Value.GetScoreboard(activity, match, cancellationToken);
            return await SendMessage(response, activity, cancellationToken);
        }

        private async Task<HttpResponseMessage> HandleKarmaChange(Activity activity, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"Message: {activity.Text}");

            // Compose the bot's response
            var reply = activity.CreateReply(string.Empty, activity.Locale);

            // Strip the mention of this bot
            Utilities.RemoveBotMentionsFromActivityText(activity);

            var replies = await _teamsKarmaLogic.Value.GetKarmaResponseTextsAsync(activity, cancellationToken);

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