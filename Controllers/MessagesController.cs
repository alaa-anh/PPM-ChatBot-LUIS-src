using System;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System.Web;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text == "login")
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    Activity replyToConversation = activity.CreateReply();
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";

                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = $"{System.Configuration.ConfigurationManager.AppSettings["AuthLogPage"]}?userid={HttpUtility.UrlEncode(activity.From.Id)}",
                        Type = "signin",
                        Title = "Authentication Required"
                    };

                    //CardAction plButton = new CardAction()
                    //{
                    //    Value = $"{System.Configuration.ConfigurationManager.AppSettings["AppWebSite"]}/Home/Login?userid={HttpUtility.UrlEncode(activity.From.Id)}",
                    //    Type = "signin",
                    //    Title = "Authentication Required"
                    //};
                    cardButtons.Add(plButton);
                    SigninCard plCard = new SigninCard("Please login to Office 365", new List<CardAction>() { plButton });
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);

                    var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
                }
               
               
                else
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply("# Bot Help\n\nType the following command. (You need your Office 365 Exchange Online subscription.)\n\nlogin -- Login to Office 365\n\nget mail -- Get your e-mail from Office 365\n\nrevoke -- Revoke permissions for accessing your e-mail");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}