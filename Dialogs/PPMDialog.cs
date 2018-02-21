using System;
using System.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using LuisBot.Model;
using Chronic;
using System.Collections.Generic;


using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Builder.ConnectorEx;
using System.Net.Http;
using System.Web.Http.Tracing;
using System.Threading;
using Microsoft.Bot.Builder.FormFlow;
using LuisBot.Forms;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class PPMDialog : LuisDialog<object>
    {
        private string userName;
        private string password;

        private DateTime msgReceivedDate;
        public PPMDialog(Activity activity) : base(new LuisService(new LuisModelAttribute(


            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            userName = activity.From.Name;
            msgReceivedDate = DateTime.Now;// activity.Timestamp ? ? DateTime.Now;
        }
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
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {
                string id;
                var userfound = context.UserData.TryGetValue("UserName", out id);
                StringBuilder response = new StringBuilder();

                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response.Append($"Good morning, {userName}.. :)");
                }
                else
                {
                    response.Append($"Hey {userName}.. :)");
                }

                await context.PostAsync(response.ToString());
                context.Wait(this.MessageReceived);
            }
            else
            {
                var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
                context.Call(form, SignUpComplete);
            }
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



        [LuisIntent("GetAllProjectsData")]
        public async Task GetAllProjectsData(IDialogContext context, LuisResult luisResult)
        {

            EntityRecommendation projects, completion;
            bool showCompletion = false;
            if (luisResult.TryFindEntity("Project.Completion", out completion))
                showCompletion = true;


            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {

                await context.PostAsync(new Common.ProjectServer(userName , password).GetAllProjects(showCompletion));
            }


            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetProjectInfo")]
        public async Task GetProjectInfo(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;
            EntityRecommendation projectIssues;


            string searchTerm_ProjectName = string.Empty;
            string ListName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (luisResult.TryFindEntity("Project.Issues", out projectIssues))
            {
                ListName = "Issues";
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
                {
                    await context.PostAsync(new Common.ProjectServer(userName, password).GetProjectSubItems(searchTerm_ProjectName , ListName));
                }
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetProjectTasks")]
        public async Task GetProjectTasks(IDialogContext context, LuisResult luisResult)
        {
            EntityRecommendation projectname;
            EntityRecommendation projectTasks;


            string searchTerm_ProjectName = string.Empty;


            if (luisResult.TryFindEntity("Project.name", out projectname) && luisResult.TryFindEntity("Project.Tasks", out projectTasks))
            {
                searchTerm_ProjectName = projectname.Entity;
            }

            if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
            {
                await context.PostAsync($"Unable to get search term.");
            }
            else
            {
                await context.PostAsync(new Common.ProjectServer(this.userName).GetProjectTasks(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        //[LuisIntent("GetProjectInfo")]
        //public async Task GetProjectInfo(IDialogContext context, LuisResult luisResult)
        //{
        //    EntityRecommendation projectname, projectSDate, projectEDate, projectDuration, projectCompletion;



        //    string searchTerm_ProjectName = string.Empty;
        //    bool Pdate = false;
        //    bool pDuration = false;
        //    bool PCompletion = false;

        //    if (luisResult.TryFindEntity("Project.name", out projectname))
        //        searchTerm_ProjectName = projectname.Entity;
        //    if (luisResult.TryFindEntity("Project.SDate", out projectSDate) || luisResult.TryFindEntity("Project.EDate", out projectEDate))
        //        Pdate = true;
        //    if (luisResult.TryFindEntity("Project.Duration", out projectDuration))
        //        pDuration = true;
        //    if (luisResult.TryFindEntity("Project.Completion", out projectCompletion))
        //        PCompletion = true;




        //    if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
        //    {
        //        await context.PostAsync($"Unable to get search term.");
        //    }
        //    else
        //    {
        //        await context.PostAsync(new Common.ProjectServer(this.userName).GetProjectInfo(searchTerm_ProjectName, Pdate, pDuration, PCompletion));
        //    }

        //    context.Wait(this.MessageReceived);
        //}

        [LuisIntent("GetProjectRiskResources")]
        public async Task GetProjectRiskResources(IDialogContext context, LuisResult luisResult)
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
                await context.PostAsync(new Common.ProjectServer(this.userName).GetProjectRiskResources(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("GetProjectRiskStatus")]
        public async Task GetProjectRiskStatus(IDialogContext context, LuisResult luisResult)
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
                await context.PostAsync(new Common.ProjectServer(this.userName).GetProjectRiskStatus(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("FilterProjectsByDate")]
        public async Task FilterProjectsByDate(IDialogContext context, LuisResult luisResult)
        {
            string FilterType = string.Empty;
            string ProjectSEdateFlag = "START";
            string ProjectED = string.Empty;


            string ProjectSDate = string.Empty;
            string ProjectEDate = string.Empty;

            var filterDate = (object)null;

            EntityRecommendation dateTimeEntity, dateRangeEntity, ProjectS, ProjectE;


            if (luisResult.TryFindEntity("builtin.datetimeV2.daterange", out dateRangeEntity))
            {
                filterDate = dateRangeEntity.Resolution.Values.Select(x => x).OfType<List<object>>().SelectMany(i => i).FirstOrDefault();
                if (Datevalues(filterDate, "Mod") != "")
                {
                    FilterType = Datevalues(filterDate, "Mod");
                    ProjectSDate = Datevalues(filterDate, "timex");

                }
                else
                {
                    FilterType = "Between";

                    ProjectSDate = Datevalues(filterDate, "start");
                }
                ProjectEDate = Datevalues(filterDate, "end");

            }
            else if (luisResult.TryFindEntity("Project.Start", out ProjectS))
            {
                ProjectSEdateFlag = "START";
            }
            else if (luisResult.TryFindEntity("Project.Finish", out ProjectE))
            {
                ProjectSEdateFlag = "Finish";
            }


            if (filterDate != null)
                await context.PostAsync(new Common.ProjectServer(this.userName).FilterProjectsByDate(FilterType, ProjectSDate, ProjectEDate, ProjectSEdateFlag));




            context.Wait(this.MessageReceived);
        }



        public string Datevalues(object obj, string keyNeed)
        {
            string keyval = string.Empty;
            if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
            {
                IDictionary idict = (IDictionary)obj;

                Dictionary<string, string> newDict = new Dictionary<string, string>();
                foreach (object key in idict.Keys)
                {
                    if (keyNeed == key.ToString())
                    {
                        keyval = idict[key].ToString();
                        //newDict.Add(key.ToString(), idict[key].ToString());
                        break;
                    }
                }
            }
            return keyval;

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
                await context.PostAsync(new Common.ProjectServer(this.userName).FindProjectByName(searchTerm_ProjectName));
            }

            context.Wait(this.MessageReceived);
        }

        private async Task SignUpComplete(IDialogContext context, IAwaitable<LoginForm> result)
        {
            LoginForm form = null;
            try
            {
                form = await result;
            }
            catch (OperationCanceledException)
            {
            }

            if (form == null)
            {
                await context.PostAsync("You canceled the form.");
            }
            else
            {
                // Here is where we could call our signup service here to complete the sign-up

                var message = $"You are currently Logged In. Please Enjoy Using our App. **{form.Name}**.";
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);
        }
        //public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var msg = await argument;
        //    if (msg.ChannelId == "ppmDL")
        //    {
        //        string botChatSecret = ConfigurationManager.AppSettings["BotChatDirectLineSecret"];
        //        var request = new HttpRequestMessage(HttpMethod.Get, "https://webchat.botframework.com/api/tokens");
        //        request.Headers.Add("Authorization", "BOTCONNECTOR " + botChatSecret);
        //        using (HttpResponseMessage response = await new HttpClient().SendAsync(request))
        //        {
        //            string token = await response.Content.ReadAsStringAsync();
        //            Token = token.Replace("\"", "");
        //        }

        //    }
        //    else if (msg.ChannelId == "facebook")
        //    {
        //        var reply = context.MakeMessage();
        //        reply.ChannelData = new FacebookMessage
        //        (
        //            text: "Please share your location with me.",
        //            quickReplies: new List<FacebookQuickReply>
        //            {
        //                // If content_type is location, title and payload are not used
        //                // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
        //                // for more information.
        //                new FacebookQuickReply(
        //                    contentType: FacebookQuickReply.ContentTypes.Location,
        //                    title: default(string),
        //                    payload: default(string)
        //                )
        //            }
        //        );
        //        await context.PostAsync(reply);
        //      //  context.Wait(LocationReceivedAsync);
        //    }
        //    else if (msg.ChannelId == "email")
        //    { }
        //    else if (msg.ChannelId == "skype")
        //    { }
        //    else if (msg.ChannelId == "slack")
        //    { }
        //    else
        //    {
        //        context.Done(default(Place));
        //    }
        //}

        //public virtual async Task LocationReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var msg = await argument;
        //    var location = msg.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
        //    context.Done(location);
        //}



    }

}


