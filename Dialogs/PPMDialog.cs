using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

using Common;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class PPMDialog : LuisDialog<object>
    {
        private string userName;
        private DateTime msgReceivedDate;

        public PPMDialog(Activity activity) : base(new LuisService(new LuisModelAttribute(


            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {

            userName = activity.From.Name;
            msgReceivedDate = DateTime.Now;// activity.Timestamp ? ? DateTime.Now;
        }

     

       
        //public PPMDialog(Activity activity)
        //{
        //    userName = activity.From.Name;
        //    msgReceivedDate = DateTime.Now;// activity.Timestamp ? ? DateTime.Now;
        //}


        [LuisIntent("")]
        [LuisIntent("none")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greet.Welcome")]
        public async Task GreetWelcome(IDialogContext context, LuisResult luisResult)
        {
            StringBuilder response = new StringBuilder();

            if (this.msgReceivedDate.ToString("tt") == "AM")
            {
                response.Append($"Good morning, {userName}.. :)");
            }
            else
            {
                response.Append($"Hey {userName}.. :)");
            }

          //  string sharepointLoginUrl = ConfigurationManager.AppSettings["SHAREPOINT_LOGIN_URI"];

          //  response.Append($"<br>Click <a href='{sharepointLoginUrl}?userName={this.userName}' >here</a> to login");

            await context.PostAsync(response.ToString());
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greet.Farewell")]
        public async Task GreetFarewell(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;
            if (this.msgReceivedDate.ToString("tt") == "AM")
            {
                response = $"Good bye, {userName}.. Have a nice day. :)";
            }
            else
            {
                response = $"b'bye {userName}, Take care.";
            }
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Search.Projects")]
        public async Task SearchProjects(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;

            string searchTerm_ProjectName = string.Empty;

            if (luisResult.TryFindEntity("Project.name", out projectname))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new ProjectServer(this.userName).FindProjectByName(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }







    }
}