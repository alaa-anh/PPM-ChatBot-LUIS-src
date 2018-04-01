﻿using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Common;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class PPMDialog : LuisDialog<object>
    {
        private string userName;
        private string password;
        private string UserLoggedInName;

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
            StringBuilder response = new StringBuilder();
            if (context.UserData.TryGetValue<string>("UserLoggedInName", out UserLoggedInName))
            {
                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response.Append($"Good morning, {UserLoggedInName}.. :)");
                }
                else
                {
                    response.Append($"Hey {UserLoggedInName}.. :)");
                }
                await context.PostAsync(response.ToString());
                context.Wait(this.MessageReceived);

            }
            else
            {
                PromptDialog.Text(
                    context: context,
                    resume: ResumeGetPassword,
                    prompt: "Dear , May I know your user name?",
                    retry: "Sorry, I didn't understand that. Please try again."
                );
            }
        }

        public virtual async Task ResumeGetPassword(IDialogContext context, IAwaitable<string> UserEmail)
        {
            string response = await UserEmail;
            userName = response; ;

            PromptDialog.Text(
                context: context,
                resume: SignUpComplete,
                prompt: "Please share your password",
                retry: "Sorry, I didn't understand that. Please try again."
            );
        }


        [LuisIntent("Greet.Farewell")]
        public async Task GreetFarewell(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;


            try
            {

                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response = $"Good bye, {UserLoggedInName}.. Have a nice day. :)";
                }
                else
                {
                    response = $"b'bye {UserLoggedInName}, Take care.";
                }


            }
            catch (Exception ex)
            {
                response = ex.Message;
            }

            context.UserData.Clear();
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }



        [LuisIntent("GetAllProjectsData")]
        public async Task GetAllProjectsData(IDialogContext context, LuisResult luisResult)
        {

            var reply = context.MakeMessage();

            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)) && (context.UserData.TryGetValue<string>("UserLoggedInName", out UserLoggedInName)))
            {
                EntityRecommendation projectSDate, projectEDate, projectDuration, projectCompletion, projectDate, projectPM;
                EntityRecommendation ProjectItemIndex;

                bool showCompletion = false;
                bool Pdate = false;
                bool pDuration = false;
                bool pPM = false;
                int itemStartIndex = 0;
                int Counter;

                if (luisResult.TryFindEntity("ItemIndex", out ProjectItemIndex))
                {
                    itemStartIndex = int.Parse(ProjectItemIndex.Entity);
                }

                if (luisResult.TryFindEntity("Project.Completion", out projectCompletion))
                    showCompletion = true;

                if (luisResult.TryFindEntity("Project.SDate", out projectSDate) || luisResult.TryFindEntity("Project.EDate", out projectEDate) || luisResult.TryFindEntity("Project.Date", out projectDate))
                    Pdate = true;

                if (luisResult.TryFindEntity("Project.Duration", out projectDuration))
                    pDuration = true;

                if (luisResult.TryFindEntity("Project.PM", out projectPM))
                    pPM = true;


                
                await context.PostAsync(new Common.ProjectServer(userName, password).GetMSProjects(context, itemStartIndex, showCompletion, Pdate, pDuration, pPM , out Counter));
                await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex,Counter ,Enums.ListName.Projects.ToString()));
                if(Counter > 10)
                    await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context , Counter , Enums.ListName.Projects.ToString() , "" , ""));

            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "You are note allwed to access the data , do you want to login?");
            }
        }


       
      

        [LuisIntent("GetProjectInfo")]
        public async Task GetProjectInfo(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {

                EntityRecommendation projectname;
                EntityRecommendation projectIssues;
                EntityRecommendation projectTasks;
                EntityRecommendation projectRisks;
                EntityRecommendation projectDeliverables;
                EntityRecommendation projectAssignments;
                EntityRecommendation ItemIndex;



                string searchTerm_ProjectName = string.Empty;
                string ListName = string.Empty;
                int itemStartIndex = 0;
                int Counter;

                if (luisResult.TryFindEntity("ItemIndex", out ItemIndex))
                {
                    itemStartIndex = int.Parse(ItemIndex.Entity);
                }

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
                    if (luisResult.TryFindEntity("Project.Issues", out projectIssues))
                    {
                        ListName = Common.Enums.ListName.Issues.ToString();
                        IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetProjectIssues(context, itemStartIndex, searchTerm_ProjectName, out Counter);
                        if (messageActivity.Attachments.Count > 0)
                        {
                            await context.PostAsync(messageActivity);
                        }
                        await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, ListName));
                        if (Counter > 10)
                            await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, ListName, searchTerm_ProjectName , ""));


                    }

                    if (luisResult.TryFindEntity("Project.Tasks", out projectTasks))
                    {
                        ListName = Common.Enums.ListName.Tasks.ToString();
                        IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetProjectTasks(context, itemStartIndex, searchTerm_ProjectName, out Counter);
                        if (messageActivity.Attachments.Count >0)
                        {
                            await context.PostAsync(messageActivity);                            
                        }
                        await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, ListName));
                        if (Counter > 10)
                            await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, ListName, searchTerm_ProjectName ,""));


                    }
                    else if (luisResult.TryFindEntity("Project.Risks", out projectRisks))
                    {
                        ListName = Common.Enums.ListName.Risks.ToString();
                        IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetProjectRisks(context, itemStartIndex, searchTerm_ProjectName, out Counter);
                        if (messageActivity.Attachments.Count > 0)
                        {
                            await context.PostAsync(messageActivity);
                        }

                        await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, ListName));
                        if (Counter > 10)
                            await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, ListName, searchTerm_ProjectName , ""));

                    }
                    else if (luisResult.TryFindEntity("Project.Deliverables", out projectDeliverables))
                    {
                        ListName = Common.Enums.ListName.Deliverables.ToString();
                        IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetProjectDeliverables(context, itemStartIndex, searchTerm_ProjectName, out Counter);
                        if (messageActivity.Attachments.Count > 0)
                        {
                            await context.PostAsync(messageActivity);
                        }
                        await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, ListName));
                        if (Counter > 10)
                            await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, ListName, searchTerm_ProjectName , ""));

                    }
                    else if (luisResult.TryFindEntity("Project.Assignments", out projectAssignments))
                    {
                        ListName = Common.Enums.ListName.Assignments.ToString();
                        IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetProjectAssignments(context, itemStartIndex, searchTerm_ProjectName, out Counter);
                        if (messageActivity.Attachments.Count > 0)
                        {
                            await context.PostAsync(messageActivity);
                        }
                        await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, ListName));
                        if (Counter > 10)
                            await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, ListName, searchTerm_ProjectName , ""));

                    }
                    else if(ListName =="")
                    {
                        EntityRecommendation projectSDate, projectEDate, projectDuration, projectCompletion, projectDate, projectManager;


                        bool Pdate = false;
                        bool pDuration = false;
                        bool PCompletion = false;
                        bool PMshow = false;


                        if (luisResult.TryFindEntity("Project.SDate", out projectSDate) || luisResult.TryFindEntity("Project.EDate", out projectEDate) || luisResult.TryFindEntity("Project.Date", out projectDate))
                            Pdate = true;
                        if (luisResult.TryFindEntity("Project.Duration", out projectDuration))
                            pDuration = true;
                        if (luisResult.TryFindEntity("Project.Completion", out projectCompletion))
                            PCompletion = true;
                        if (luisResult.TryFindEntity("Project.PM", out projectManager))
                            PMshow = true;

                        await context.PostAsync(new Common.ProjectServer(userName, password).GetProjectInfo(context, searchTerm_ProjectName, Pdate, pDuration, PCompletion, PMshow));


                    }

                }
               
                
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "You are note allwed to access the data , do you want to login?");

            }
        }

        [LuisIntent("FilterProjectsByDate")]
        public async Task FilterProjectsByDate(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {
                string FilterType = string.Empty;
                string ProjectSEdateFlag = "START";
                string ProjectED = string.Empty;


                string ProjectSDate = string.Empty;
                string ProjectEDate = string.Empty;

                var filterDate = (object)null;

                EntityRecommendation dateTimeEntity, dateRangeEntity, ProjectS, ProjectE;
                EntityRecommendation ItemIndex;

                int itemStartIndex = 0;
                int Counter;

                if (luisResult.TryFindEntity("builtin.datetimeV2.daterange", out dateRangeEntity))
                {
                    filterDate = dateRangeEntity.Resolution.Values.Select(x => x).OfType<List<object>>().SelectMany(i => i).FirstOrDefault();
                    if (Common.TokenHelper.Datevalues(filterDate, "Mod") != "")
                    {
                        FilterType = Common.TokenHelper.Datevalues(filterDate, "Mod");
                        ProjectSDate = Common.TokenHelper.Datevalues(filterDate, "timex");

                    }
                    else
                    {
                        FilterType = "Between";

                        ProjectSDate = Common.TokenHelper.Datevalues(filterDate, "start");
                    }
                    ProjectEDate = Common.TokenHelper.Datevalues(filterDate, "end");

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
                {
                    IMessageActivity messageActivity = new Common.ProjectServer(userName, password).FilterProjectsByDate(context, FilterType, ProjectSDate, ProjectEDate, ProjectSEdateFlag , out Counter);
                    if (messageActivity.Attachments.Count > 0)
                    {
                        await context.PostAsync(messageActivity);
                    }
                    await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, "FilterByDate"));
                    if (Counter > 10)
                        await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, "FilterByDate", "" , luisResult.Query));
                }
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "You are note allwed to access the data , do you want to login?");

            }
        }

        [LuisIntent("GetResourceAssignments")]
        public async Task GetResourceAssignments(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {


                EntityRecommendation resoursename, resourceassignment;
                EntityRecommendation ItemIndex;


                string searchTerm_ResourceName = string.Empty;
                string ListName = string.Empty;
                int itemStartIndex = 0;
                int Counter;


                if (luisResult.TryFindEntity("ItemIndex", out ItemIndex))
                {
                    itemStartIndex = int.Parse(ItemIndex.Entity);
                }
                if (luisResult.TryFindEntity("user.name", out resoursename))
                {
                    searchTerm_ResourceName = resoursename.Entity;
                }

               

                if (string.IsNullOrWhiteSpace(searchTerm_ResourceName))
                {
                    await context.PostAsync($"Unable to get search term.");
                }
                else
                {
                    IMessageActivity messageActivity = new Common.ProjectServer(userName, password).GetResourceAssignments(context, itemStartIndex, searchTerm_ResourceName, out Counter);
                    if (messageActivity.Attachments.Count > 0)
                    {
                        await context.PostAsync(messageActivity);
                    }
                    await context.PostAsync(new Common.ProjectServer(userName, password).TotalCountGeneralMessage(context, itemStartIndex, Counter, "UserAssignments"));
                    if (Counter > 10)
                        await context.PostAsync(new Common.ProjectServer(userName, password).CreateButtonsPager(context, Counter, "UserAssignments", searchTerm_ResourceName, luisResult.Query));

                }
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "You are note allwed to access the data , do you want to login?");

            }
        }


        

        public virtual async Task SignUpComplete(IDialogContext context, IAwaitable<string> pass)
        {
            string response = await pass;
            password = response;

           
            string UserLoggedInName = TokenHelper.checkAuthorizedUser(userName, password);
            if (UserLoggedInName != string.Empty)
            {
                context.UserData.SetValue("UserName", userName);
                context.UserData.SetValue("Password", password);
                context.UserData.SetValue("UserLoggedInName", UserLoggedInName);


                var message = $"You are currently Logged In. Please Enjoy Using our App. **{UserLoggedInName}**.";
                await context.PostAsync(message);
            }
            else
            {
                PromptDialog.Confirm(context, ResumeAfterConfirmation, "The User Don't have permission , do you want to try another cridentials?");

            }
        }

        private async Task ResumeAfterConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmation = await result;
            if (confirmation == true)
            {
                PromptDialog.Text(
                    context: context,
                    resume: ResumeGetPassword,
                    //pattern : @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                    prompt: "Dear , May I know your user name?",
                    retry: "Sorry, I didn't understand that. Please try again."
                );
            }
            else
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

                context.UserData.Clear();
                await context.PostAsync(response);
                context.Wait(this.MessageReceived);
            }
            //  await context.PostAsync(confirmation ? "You do want to order." : "You don't want to order.");
        }

        
      


    }

}


