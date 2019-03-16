using System.Net;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace MSTeams.Karma.Controllers
{
    /// <summary>
    /// Main messaging controller.
    /// </summary>
    /// <seealso cref="ApiController" />
    [BotAuthentication, TenantFilter]
    public class MessagesController : ApiController
    {
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
            var replyMessage = KarmaLogic.GetReplyMessageForKarma(activity.Text);
            if (string.IsNullOrEmpty(replyMessage))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            reply.Text += replyMessage;

            // Send the response
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connectorClient.Conversations.ReplyToActivityAsync(reply, cancellationToken);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}