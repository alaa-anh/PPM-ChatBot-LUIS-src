using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

namespace Common
{
    public class ProjectServer
    {
        private string _userName;
        private string _userPassword;

        private string _siteUri;
        public ProjectServer(string userName, string password)
        {
            _userName = userName;
            _userPassword = password;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }


        public IMessageActivity GetAllProjects(IDialogContext dialogContext,int SIndex, bool showCompletion, bool ProjectDates, bool PDuration, bool projectManager , out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);

                context.Load(context.Projects);
                context.ExecuteQuery();
                int inDexToVal = SIndex + 10;
                Counter = context.Projects.Count;
                if (inDexToVal >= context.Projects.Count)
                    inDexToVal = context.Projects.Count;
                ProjectCollection projectDetails = context.Projects;



                if (context.Projects.Count > 0)
                {

                    for (int startIndex = SIndex; startIndex < inDexToVal; startIndex++)
                    {
                        PublishedProject pro = context.Projects[startIndex];
                        context.Load(pro.Owner);
                        context.Load(pro, p => p.ProjectSiteUrl);
                        context.ExecuteQuery();

                        string ProjectName = pro.Name;
                        string ProjectWorkspaceInternalUrl = pro.ProjectSiteUrl;
                        string ProjectPercentCompleted = pro.PercentComplete.ToString();
                        string ProjectFinishDate = pro.FinishDate.ToString();
                        string ProjectStartDate = pro.StartDate.ToString();
                        TimeSpan duration = pro.FinishDate - pro.StartDate;

                        string ProjectDuration = duration.Days.ToString();

                        string ProjectOwnerName = pro.Owner.Title;


                        string SubtitleVal = "";

                        if (showCompletion == false && ProjectDates == false && PDuration == false && projectManager == false)
                        {
                            SubtitleVal += "**Completed Percentage :**\n" + ProjectPercentCompleted + "%\n\r";
                            SubtitleVal += "**Start Date :**\n" + ProjectStartDate + "\n\r";
                            SubtitleVal += "**Finish Date :**\n" + ProjectFinishDate + "\n\r";
                            SubtitleVal += "**Project Duration :**\n" + ProjectDuration + "\n\r";
                            SubtitleVal += "**Project Manager :**\n" + ProjectOwnerName + "\n\r";
                        }
                        else if (showCompletion == true)
                            SubtitleVal += "**Completed Percentage :**\n" + ProjectPercentCompleted + "%\n\r";

                        else if (ProjectDates == true)
                        {
                            SubtitleVal += "**Start Date :**\n" + ProjectStartDate + "\n\r";
                            SubtitleVal += "**Finish Date :**\n" + ProjectFinishDate + "\n\r";
                        }

                        else if (PDuration == true)
                        {
                            SubtitleVal += "**Project Duration :**\n" + ProjectDuration + "\n\r";
                        }


                        else if (projectManager == true)
                        {
                            SubtitleVal += "**Project Manager :**\n" + ProjectOwnerName + "\n\r";
                        }

                        string ImageURL = "http://02-code.com/images/logo.jpg";

                        List<CardImage> cardImages = new List<CardImage>();
                        List<CardAction> cardactions = new List<CardAction>();

                        cardImages.Add(new CardImage(url: ImageURL));


                        CardAction btnWebsite = new CardAction()
                        {
                            Type = ActionTypes.OpenUrl,
                            Title = "Open",
                            Value = ProjectWorkspaceInternalUrl + "?redirect_uri={" + ProjectWorkspaceInternalUrl + "}",
                        };

                        CardAction btnTasks = new CardAction()
                        {
                            Type = ActionTypes.PostBack,
                            Title = "Tasks",
                            Value = "show a list of " + ProjectName + " tasks",
                        };
                        cardactions.Add(btnTasks);

                        CardAction btnIssues = new CardAction()
                        {
                            Type = ActionTypes.PostBack,
                            Title = "Issues",
                            Value = "show a list of " + ProjectName + " issues",
                        };
                        cardactions.Add(btnIssues);


                        CardAction btnRisks = new CardAction()
                        {
                            Type = ActionTypes.PostBack,
                            Title = "Risks",
                            Value = "Show risks and the assigned resources of " + ProjectName,
                        };
                        cardactions.Add(btnRisks);


                        CardAction btnDeliverables = new CardAction()
                        {
                            Type = ActionTypes.PostBack,
                            Title = "Deliverables",
                            Value = "Show "+ProjectName+" deliverables",
                        };
                        cardactions.Add(btnDeliverables);

                        CardAction btnDAssignments = new CardAction()
                        {
                            Type = ActionTypes.PostBack,
                            Title = "Assignments",
                            Value = "get "+ProjectName+" assignments",
                        };
                        cardactions.Add(btnDAssignments);

                        HeroCard plCard = new HeroCard()
                        {
                            Title = ProjectName,
                            Subtitle = SubtitleVal,
                            Images = cardImages,
                            Buttons = cardactions,
                            Tap = btnTasks,

                        };                      
                        reply.Attachments.Add(plCard.ToAttachment());
                    }


                }

            }

            return reply;
        }

        public IMessageActivity GetProjectTasks(IDialogContext dialogContext , int itemStartIndex, string pName ,  out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            
            int TaskCounter = 0;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);

               

               


                if (project != null)
                {
                    context.Load(project.Tasks);
                    context.ExecuteQuery();
                    PublishedTaskCollection publishedTask = project.Tasks;
                    if (project.Tasks.Count > 0)
                    {
                        if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
                        {
                            reply = GetResourceLoggedInTasks(dialogContext, context, project);
                        }
                        else if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)"))
                        {
                            //  context.Load(project.Owner);
                            //context.ExecuteQuery();
                            //if (project.Owner.Email == _userName) // if the logged in user is a project manager on this project
                            //{
                            reply = GetAllTasks(dialogContext, itemStartIndex, publishedTask, project, out TaskCounter);
                            //}
                            //else
                            //{
                            //    reply = GetResourceLoggedInTasks(dialogContext, context, project);

                            //}
                        }
                        else
                        {
                            reply = GetAllTasks(dialogContext, itemStartIndex, publishedTask, project, out TaskCounter);
                        }
                    }
                }
            }
            Counter = TaskCounter;
            return reply;
        }

        public IMessageActivity GetProjectIssues(IDialogContext dialogContext, int itemStartIndex, string pName, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string projectsite = string.Empty;
            Web projectweb;
            int TaskCounter = 0;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);
                if (project != null)
                {
                    string IssueName = string.Empty;
                    string IssueStatus = string.Empty;
                    string IssuePriority = string.Empty;

                    context.Load(project, p => p.ProjectSiteUrl);
                    context.ExecuteQuery();

                    projectsite = project.ProjectSiteUrl;
                    projectweb = GetProjectWEB(projectsite, context);

                    var issues = projectweb.Lists.GetByTitle(Enums.ListName.Issues.ToString());
                    CamlQuery query = CamlQuery.CreateAllItemsQuery();
                    ListItemCollection itemsIssue = issues.GetItems(query);

                    context.Load(issues);
                    context.Load(itemsIssue);
                    context.ExecuteQuery();

                    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
                    {
                       // reply = getresourceassignedrisksIssues(dialogContext, itemsIssue);
                    }
                    else
                    {
                        reply = GetAllIssues(dialogContext, itemsIssue , itemStartIndex, out TaskCounter);
                    }
                }
            }
            Counter = TaskCounter;
            return reply;
        }

        public IMessageActivity GetProjectRisks(IDialogContext dialogContext, int itemStartIndex, string pName, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string projectsite = string.Empty;
            Web projectweb;
            int TaskCounter = 0;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);

                if (project != null)
                {
                    context.Load(project, p => p.ProjectSiteUrl);
                    context.ExecuteQuery();

                    projectsite = project.ProjectSiteUrl;
                    projectweb = GetProjectWEB(projectsite, context);

                    var risks = projectweb.Lists.GetByTitle(Enums.ListName.Risks.ToString());
                    CamlQuery query = CamlQuery.CreateAllItemsQuery();
                    ListItemCollection itemsRisk = risks.GetItems(query);

                    projectweb.Context.Load(risks);
                    projectweb.Context.Load(itemsRisk);
                    projectweb.Context.ExecuteQuery();

                    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
                    {
                      //  markdownContent = getresourceassignedrisks(itemsRisk);
                    }
                    else if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)"))
                    {
                      //  markdownContent = getallrisks(itemsRisk);
                    }
                    else
                    {
                        reply = GetAllRisks(dialogContext, itemsRisk, itemStartIndex, out TaskCounter);
                    }

                    


                }
            }
            Counter = TaskCounter;

            return reply;
        }

        public IMessageActivity GetProjectDeliverables(IDialogContext dialogContext, int itemStartIndex, string pName, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string projectsite = string.Empty;
            Web projectweb;
            int TaskCounter = 0;
          
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);

                if (project != null)
                {
                    context.Load(project, p => p.ProjectSiteUrl);
                    context.ExecuteQuery();

                    projectsite = project.ProjectSiteUrl;
                    projectweb = GetProjectWEB(projectsite, context);

                    var delive = projectweb.Lists.GetByTitle(Enums.ListName.Deliverables.ToString());
                    CamlQuery query = CamlQuery.CreateAllItemsQuery();
                    ListItemCollection itemsdelive = delive.GetItems(query);

                    projectweb.Context.Load(delive);
                    projectweb.Context.Load(itemsdelive);
                    projectweb.Context.ExecuteQuery();

                    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
                    {
                        //  markdownContent = getresourceassignedrisks(itemsRisk);
                    }
                    else if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)"))
                    {
                        //  markdownContent = getallrisks(itemsRisk);
                    }
                    else
                    {
                        reply = GetAllDeliverabels(dialogContext, itemsdelive, itemStartIndex, out TaskCounter);
                    }
                  
                }
            }
            Counter = TaskCounter;

            return reply;
        }

        public IMessageActivity GetProjectAssignments(IDialogContext dialogContext, int itemStartIndex, string pName, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string projectsite = string.Empty;
            Web projectweb;
            int TaskCounter = 0;

            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);
                Counter = 0;
                if (project != null)
                {
                    context.Load(project.Assignments);
                    context.ExecuteQuery();
                    PublishedAssignmentCollection asscoll = project.Assignments;
                    Counter = asscoll.Count;
                    if (asscoll.Count > 0)
                    {

                        

                        int inDexToVal = itemStartIndex + 10;
                        if (inDexToVal >= asscoll.Count)
                            inDexToVal = asscoll.Count;

                        for (int startIndex = itemStartIndex; startIndex < inDexToVal; startIndex++)
                        {
                            PublishedAssignment ass = asscoll[startIndex];
                            string SubtitleVal = ""; 
                            context.Load(ass.Task);
                            context.Load(ass.Resource);
                            context.ExecuteQuery();

                            string TaskName = ass.Task.Name;
                            SubtitleVal += "**Resource Name :**\n" + ass.Resource.Name + "\n\r";
                            SubtitleVal += "**Start Date**\n" + ass.Start + "\n\r";
                            SubtitleVal += "**Finish Date**\n" + ass.Finish + "\n\r";


                            HeroCard plCard = new HeroCard()
                            {
                                Title = TaskName,
                                Subtitle = SubtitleVal,
                            };
                            reply.Attachments.Add(plCard.ToAttachment());
                        }
                    }
                }
            }
            return reply;
        }

        public IMessageActivity FilterProjectsByDate(IDialogContext dialogContext, string FilterType, string pStartDate, string PEndDate, string ProjectSEdateFlag , out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            Counter = 0;
            IEnumerable<PublishedProject> retrivedProjects = null; ;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                DateTime startdate = new DateTime();
                DateTime endate = new DateTime();

                if (!string.IsNullOrEmpty(pStartDate))
                    startdate = DateTime.Parse(pStartDate);

                if (!string.IsNullOrEmpty(PEndDate))
                    endate = DateTime.Parse(PEndDate);


                if (ProjectSEdateFlag == "START")
                {

                    if (FilterType.ToUpper() == "BEFORE" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                                  .Where(p => (p.StartDate <= startdate))
                                  .Select(p => p);
                        retrivedProjects = context.LoadQuery(pubProjects);


                    }
                    else if (FilterType.ToUpper() == "AFTER" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= startdate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }

                    else if (FilterType.ToUpper() == "BETWEEN" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= startdate && p.StartDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                }
                else
                {

                    if (FilterType.ToUpper() == "BEFORE" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.FinishDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }

                    else if (FilterType.ToUpper() == "AFTER" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                    else if (FilterType.ToUpper() == "BETWEEN" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.FinishDate >= startdate && p.FinishDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                }

                context.ExecuteQuery();
                if (retrivedProjects.Count() > 0)
                {
                    Counter = retrivedProjects.Count();
                    foreach (var item in retrivedProjects)
                    {
                        string SubtitleVal = "";


                        string ProjectName = item.Name;
                        SubtitleVal += "**Start Date**\n" + item.StartDate + "\n\r";
                        SubtitleVal += "**Finish Date**\n" + item.FinishDate + "\n\r";
                        SubtitleVal += "**Actual Cost**\n" + item.DefaultFixedCostAccrual.ToString() + "\n\r";

                        HeroCard plCard = new HeroCard()
                        {
                            Title = ProjectName,
                            Subtitle = SubtitleVal,
                        };
                        reply.Attachments.Add(plCard.ToAttachment());


                    }
                }
            }





            return reply;
        }

        public IMessageActivity GetProjectInfo(IDialogContext dialogContext, string pName, bool optionalDate = false, bool optionalDuration = false, bool optionalCompletion = false, bool optionalPM = false)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string SubtitleVal = "";

            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);

                PublishedProject project = GetProjectByName(pName, context);

                if (project != null)
                {
                    if (optionalDate == true)
                    {
                        SubtitleVal += "**Start Date :**\n" + project.StartDate + "\n\r";
                        SubtitleVal += "**Finish Date :**\n" + project.FinishDate + "\n\r";
                    }

                    if (optionalDuration == true)
                    {
                        TimeSpan duration = project.FinishDate - project.StartDate;
                        SubtitleVal += "**Project Duration :**\n" + duration.Days + "\n\r";
                    }

                    if (optionalCompletion == true)
                        SubtitleVal += "**Project Completed Percentage :**\n" + project.PercentComplete + "%\n\r";

                    if (optionalPM == true)
                    {
                        if (GetUserGroup(context, "Team Members (Project Web App Synchronized)") == false)
                        {
                            context.Load(project.Owner);
                            context.ExecuteQuery();
                            SubtitleVal += "**Project Manager Name :**\n" + project.Owner.Title + "\n\r";
                        }
                    }

                    HeroCard plCard = new HeroCard()
                    {
                        Title = pName,
                        Subtitle = SubtitleVal,
                    };
                    reply.Attachments.Add(plCard.ToAttachment());
                    
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    {
                        Title = "Project Name Not Exist",
                    };
                    reply.Attachments.Add(plCardNoData.ToAttachment());

                }


              


            }
            return reply;
        }

        public IMessageActivity TotalCountGeneralMessage(IDialogContext dialogContext, int SIndex, int Counter , string ListName)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            if (ListName == Enums.ListName.Projects.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 projects";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 projects";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Projects :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Projects :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Projects**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == Enums.ListName.Tasks.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Tasks";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Tasks";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Tasks :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Tasks :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Tasks**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == Enums.ListName.Issues.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Issues";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Issues";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Issues :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Issues :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Issues**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == Enums.ListName.Assignments.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Assignments";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Assignments";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Assignments :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Assignments :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Assignments**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == Enums.ListName.Risks.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Risks";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Risks";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Risks :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Risks :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Risks**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == Enums.ListName.Deliverables.ToString())
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Deliverables";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Deliverables";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Deliverables :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Deliverables :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Deliverables**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }
            else if (ListName == "FilterByDate")
            {
                if (Counter > 0)
                {
                    if (Counter >= 10)
                    {
                        string subTitle = string.Empty;
                        if (SIndex == 0)
                            subTitle = "You are viwing the first page , each page view 10 Projects";
                        else if (SIndex > 0)
                        {
                            int pagenumber = SIndex / 10 + 1;
                            subTitle = "You are viwing the page number " + pagenumber + " , each page view 10 Deliverables";
                        }
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Projects :**\n" + Counter,
                            Subtitle = subTitle,
                            //  Buttons = cardButtons,
                        };
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                    else
                    {
                        HeroCard plCardCounter = new HeroCard()
                        {
                            Title = "**Total Number Of availabel Projects :**\n" + Counter,
                        };
                        reply.Attachments.Add(plCardCounter.ToAttachment());
                    }
                }
                else
                {
                    HeroCard plCardNoData = new HeroCard()
                    { Title = "**No Availabel Projects**\n\n" };
                    reply.Attachments.Add(plCardNoData.ToAttachment());
                }
            }

            
            return reply;
        }

        public IMessageActivity CreateButtonsPager(IDialogContext dialogContext, int totalCount , string ListName , string projectName , string query)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            if (totalCount > 10)
            {
                List<CardAction> cardButtons = new List<CardAction>();
                double p = totalCount * 0.1;
                double result = Math.Ceiling(p);
                int pagenumber = int.Parse(result.ToString());

                string valuebutton = string.Empty;
                for (int i = 0; i < pagenumber; i++)
                {
                    string CurrentNumber = Convert.ToString(i);
                    if (projectName == "")
                    {
                        if (i == 0)
                        {
                            valuebutton = "get all projects at index 0";
                        }
                        else
                            valuebutton = "get all projects at index " + i * 10;
                    }
                    else if (ListName == Enums.ListName.Tasks.ToString())
                    {
                        if (i == 0)
                        {
                            valuebutton = "show a list of "+projectName+" tasks at index 0";
                        }
                        else
                            valuebutton = "show a list of "+ projectName + " tasks at index " + i * 10;

                    }
                    else if (ListName == Enums.ListName.Issues.ToString())
                    {
                        if (i == 0)
                        {
                            valuebutton = "show a list of " + projectName + " issues at index 0";
                        }
                        else
                            valuebutton = "show a list of " + projectName + " issues at index " + i * 10;

                    }
                    else if (ListName == Enums.ListName.Deliverables.ToString())
                    {
                        if (i == 0)
                        {
                            valuebutton = "show a list of " + projectName + " deliverables at index 0";
                        }
                        else
                            valuebutton = "show a list of " + projectName + " deliverables at index " + i * 10;

                    }
                    else if (ListName == Enums.ListName.Risks.ToString())
                    {
                        if (i == 0)
                        {
                            valuebutton = "show a list of " + projectName + " risks at index 0";
                        }
                        else
                            valuebutton = "show a list of " + projectName + " risks at index " + i * 10;

                    }
                    else if (ListName == Enums.ListName.Assignments.ToString())
                    {
                        if (i == 0)
                        {
                            valuebutton = "get " + projectName + " assignments at index 0";
                        }
                        else
                            valuebutton = "get " + projectName + " assignments at index " + i * 10;

                    }
                    else if (ListName == "FilterByDate" && query !="")
                    {
                        if (i == 0)
                        {
                            valuebutton = query + " at index 0";
                        }
                        else
                            valuebutton = query + " at index " + i * 10;

                    }

                    CurrentNumber = Convert.ToString(i + 1);
                    CardAction CardButton = new CardAction()
                    {
                        Type = ActionTypes.PostBack,
                        Title = CurrentNumber,
                        Value = valuebutton,
                    };
                    // cardButtons.Add(CardButton);


                    //List<CardImage> cardImages = new List<CardImage>();

                    //cardImages.Add(new CardImage(url: "http://www.kidsmathgamesonline.com/images/pictures/numbers600/number" + i + ".jpg"));


                    ThumbnailCard plCardCounter = new ThumbnailCard()
                    {
                         Title = "Page" + CurrentNumber,
                     //   Images = cardImages,
                        Tap = CardButton,

                    };

                    reply.Attachments.Add(plCardCounter.ToAttachment());
                }
            }




            return reply;
        }

        public IMessageActivity GetProjectSubItems(IDialogContext dialogContext ,  string pName, string ListName)
        {
            var markdownContent = "";
            string SubtitleVal = "";
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();

            string projectsite = string.Empty;
            Web projectweb;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName(pName, context);                
                if (project != null)
                {
                    context.Load(project, p => p.ProjectSiteUrl);
                    context.ExecuteQuery();

                    if (ListName == Enums.ListName.Tasks.ToString())
                    {
                       // reply = GetProjectTasks(dialogContext , context, project);
                    }
                    else if (ListName == Enums.ListName.Assignments.ToString())
                    {
                      //  markdownContent = GetProjectTAssignments(context, project);
                    }
                    else
                    {
                        projectsite = project.ProjectSiteUrl;
                        projectweb = GetProjectWEB(projectsite, context);

                        if (projectsite != null)
                        {
                           // if (ListName == Common.Enums.ListName.Issues.ToString())
                             //   markdownContent = GetProjectIssues( dialogContext, projectweb, context);
                          //  if (ListName == Common.Enums.ListName.Risks.ToString())
                             //   markdownContent = GetProjectRisks(projectweb, context);
                           // if (ListName == Common.Enums.ListName.Deliverables.ToString())
                              //  markdownContent = GetProjectDeliverables(projectweb, context);
                        }
                        else
                        {
                            markdownContent = "Site Project Not Created";
                        }
                    }

                }
                else
                {
                    markdownContent = "Project Name Not Exist or you don't have permission to this project";
                }

            }
            return reply;
        }





        public IMessageActivity GetAllTasks(IDialogContext dialogContext, int SIndex, PublishedTaskCollection tskcoll, PublishedProject project, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;


            int inDexToVal = SIndex + 10;
            Counter = project.Tasks.Count;
            if (inDexToVal >= project.Tasks.Count)
                inDexToVal = project.Tasks.Count;

            tskcoll = project.Tasks;

            if (tskcoll.Count > 0)
            {
                for (int startIndex = SIndex; startIndex < inDexToVal; startIndex++)
                {
                    var SubtitleVal = "";
                    PublishedTask tsk = tskcoll[startIndex];
                    string TaskName = tsk.Name;
                    string TaskDuration = tsk.Duration;
                    string TaskPercentCompleted = tsk.PercentComplete.ToString();
                    string TaskStartDate = tsk.Start.ToString();
                    string TaskFinishDate = tsk.Finish.ToString();

                    SubtitleVal += "**Task Duration**\n" + TaskDuration + "\n\r";
                    SubtitleVal += "**Task Percent Completed**\n" + TaskPercentCompleted + "\n\r";
                    SubtitleVal += "**Task Start Date**\n" + TaskStartDate + "\n\r";
                    SubtitleVal += "**Task Finish Date**\n" + TaskFinishDate + "\n\r";

                    HeroCard plCard = new HeroCard()
                    {
                        Title = TaskName,
                        Subtitle = SubtitleVal
                    };

                    reply.Attachments.Add(plCard.ToAttachment());
                }
               
            }
           
            return reply;
        }

        public IMessageActivity GetAllIssues(IDialogContext dialogContext , ListItemCollection itemsIssue , int SIndex,  out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string IssueName = string.Empty;
            string IssueStatus = string.Empty;
            string IssuePriority = string.Empty;

            Counter = itemsIssue.Count;

            int inDexToVal = SIndex + 10;
            if (inDexToVal >= itemsIssue.Count)
                inDexToVal = itemsIssue.Count;

            if (itemsIssue.Count > 0)
            {
                for (int startIndex = SIndex; startIndex < inDexToVal; startIndex++)
                {
                    var SubtitleVal = "";
                    ListItem item = itemsIssue[startIndex];
                    if (item["Title"] != null)
                        IssueName = (string)item["Title"];
                    if (item["Status"] != null)
                        IssueStatus = (string)item["Status"];
                    if (item["Priority"] != null)
                        IssuePriority = (string)item["Priority"];
                    SubtitleVal += "**Status**\n" + IssueStatus + "\n\r";
                    SubtitleVal += "**Priority**\n" + IssuePriority + "\n\r";

                    HeroCard plCard = new HeroCard()
                    {
                        Title = IssueName,
                        Subtitle = SubtitleVal,
                    };
                    reply.Attachments.Add(plCard.ToAttachment());
                }
            }
            return reply;
        }

        public IMessageActivity GetAllRisks(IDialogContext dialogContext, ListItemCollection itemsRisk, int SIndex, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string RiskName = string.Empty;
            string ResourceName = string.Empty;
            string riskStatus = string.Empty;
            string riskImpact = string.Empty;
            string riskProbability = string.Empty;
            string riskCostExposure = string.Empty;


            Counter = itemsRisk.Count;

            int inDexToVal = SIndex + 10;
            if (inDexToVal >= itemsRisk.Count)
                inDexToVal = itemsRisk.Count;

            if (itemsRisk.Count > 0)
            {
                for (int startIndex = SIndex; startIndex < inDexToVal; startIndex++)
                {
                    var SubtitleVal = "";
                    ListItem item = itemsRisk[startIndex];


                    if (item["Title"] != null)
                        RiskName = (string)item["Title"];
                    if (item["AssignedTo"] != null)
                    {
                        FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];
                        SubtitleVal += "**Assigned To Resource**\n" + fuv.LookupValue + "\n\r";

                    }
                    else
                        SubtitleVal += "**Assigned To Resource :**\n" + "Not assigned" + "\n\r";

                    if (item["Status"] != null)
                        riskStatus = (string)item["Status"];
                    SubtitleVal += "**Risk Status**\n" + riskStatus + "\n\r";

                    if (item["Impact"] != null)
                        riskImpact = item["Impact"].ToString();
                    SubtitleVal += "**Risk Impact**\n" + riskImpact + "\n\r";

                    if (item["Probability"] != null)
                        riskProbability = item["Probability"].ToString();
                    SubtitleVal += "**Risk Probability**\n" + riskProbability + "\n\r";

                    if (item["Exposure"] != null)
                        riskCostExposure = item["Exposure"].ToString();
                    SubtitleVal += "**Risk CostExposure**\n" + riskCostExposure + "\n\r";


                    HeroCard plCard = new HeroCard()
                    {
                        Title = RiskName,
                        Subtitle = SubtitleVal,
                    };
                    reply.Attachments.Add(plCard.ToAttachment());

                }

            }

            return reply;
        }


        public IMessageActivity GetAllDeliverabels(IDialogContext dialogContext, ListItemCollection itemsDeliverabels, int SIndex, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            string DeliverableName = string.Empty;
            string DeliverableStart = string.Empty;
            string DeliverableFinish = string.Empty;


            Counter = itemsDeliverabels.Count;

            int inDexToVal = SIndex + 10;
            if (inDexToVal >= itemsDeliverabels.Count)
                inDexToVal = itemsDeliverabels.Count;

            if (itemsDeliverabels.Count > 0)
            {
                for (int startIndex = SIndex; startIndex < inDexToVal; startIndex++)
                {
                    var SubtitleVal = "";
                    ListItem item = itemsDeliverabels[startIndex];


                    if (item["Title"] != null)
                        DeliverableName = (string)item["Title"];
                    SubtitleVal += "**Deliverable Name**\n" + DeliverableName + "\n\r";

                    if (item["Author"] != null)
                    {
                        FieldUserValue fuv = (FieldUserValue)item["Author"];
                        SubtitleVal += "**Create By Resource :**\n" + fuv.LookupValue + "\n\r";

                    }

                    if (item["CommitmentStart"] != null)
                        DeliverableStart = item["CommitmentStart"].ToString();
                    SubtitleVal += "**Start Date :**\n" + DeliverableStart + "\n\r";

                    if (item["CommitmentFinish"] != null)
                        DeliverableFinish = item["CommitmentFinish"].ToString();
                    SubtitleVal += "**Finish Date :**\n" + DeliverableFinish + "\n\r";


                    HeroCard plCard = new HeroCard()
                    {
                        Title = DeliverableName,
                        Subtitle = SubtitleVal,
                    };
                    reply.Attachments.Add(plCard.ToAttachment());

                }

            }

            return reply;
        }
        public string getresourceassignedrisks(ListItemCollection itemsRisk)
        {
            var markdownContent = "";
            string RiskName = string.Empty;
            string ResourceName = string.Empty;
            string riskStatus = string.Empty;
            string riskImpact = string.Empty;
            string riskProbability = string.Empty;
            string riskCostExposure = string.Empty;
            if (itemsRisk.Count > 0)
            {
                int count = 0;
                foreach (ListItem item in itemsRisk)
                {


                    if (item["AssignedTo"] != null)
                    {
                        count++;
                        FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];
                        if (fuv.Email == _userName)
                        {
                            if (item["Title"] != null)
                                RiskName = (string)item["Title"];
                            markdownContent += "**Risk Title**\n" + RiskName + "<br>";

                            markdownContent += "**Assigned To Resource**\n" + fuv.LookupValue + "<br/>";
                            if (item["Status"] != null)
                                riskStatus = (string)item["Status"];
                            markdownContent += "**Risk Status**\n" + riskStatus + "<br>";

                            if (item["Impact"] != null)
                                riskImpact = item["Impact"].ToString();
                            markdownContent += "**Risk Impact**\n" + riskImpact + "<br>";

                            if (item["Probability"] != null)
                                riskProbability = item["Probability"].ToString();
                            markdownContent += "**Risk Probability**\n" + riskProbability + "<br>";

                            if (item["Exposure"] != null)
                                riskCostExposure = item["Exposure"].ToString();
                            markdownContent += "**Risk CostExposure**\n" + riskCostExposure + "<br>";


                            markdownContent += "----\n\n";
                        }

                    }


                }

                markdownContent += "**Total Risks :**\n" + count + "<br>";
            }
            else
                markdownContent = "No Risks assigned for you on this project";

            return markdownContent;
        }
        


        //public string GetProjectIssues( IDialogContext dialogContext, Web projectweb, ProjectContext context)
        //{
        //    IMessageActivity reply = null;
        //    reply = dialogContext.MakeMessage();
        //    string markdownContent = string.Empty;
        //    string IssueName = string.Empty;
        //    string IssueStatus = string.Empty;
        //    string IssuePriority = string.Empty;

        //    var issues = projectweb.Lists.GetByTitle(Enums.ListName.Issues.ToString());
        //    CamlQuery query = CamlQuery.CreateAllItemsQuery();
        //    ListItemCollection itemsIssue = issues.GetItems(query);

        //    context.Load(issues);
        //    context.Load(itemsIssue);
        //    context.ExecuteQuery();

        //    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
        //    {
        //        markdownContent = getresourceassignedrisksIssues(dialogContext, itemsIssue);
        //    }
        //    else
        //    {
        //        markdownContent = GetAllProjectIssues(dialogContext, itemsIssue);
        //    }



        //    return markdownContent;
        //}

        //public IMessageActivity GetProjectTasks(IDialogContext dialogContext ,  ProjectContext context, PublishedProject project)
        //{
        //    IMessageActivity reply = null;
        //    reply = dialogContext.MakeMessage();
        //    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
        //    {
        //        reply = GetResourceLoggedInTasks(dialogContext, context, project);
        //    }
        //    else if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)"))
        //    {
        //        context.Load(project.Owner);
        //        context.ExecuteQuery();
        //        if (project.Owner.Email == _userName) // if the logged in user is a project manager on this project
        //        {
        //            reply = GetAllTasks(dialogContext , 0, context, project);
        //        }
        //        else
        //        {
        //            reply = GetResourceLoggedInTasks(dialogContext, context, project);

        //        }
        //    }
        //    else
        //    {
        //        reply = GetAllTasks(dialogContext, 0, context, project);
        //    }



        //    return reply;
        //}

       
        
        public bool UserHavePermissionOnaProjects(string siteUrl, string subSiteTitle, ProjectContext context)
        {

            var web = context.Web;
            bool exist = false;
            context.Load(web, w => w.Webs);
            context.ExecuteQuery();
            foreach (Web subWeb in web.Webs)
            {
                if (subWeb.Title.ToLower() == subSiteTitle.ToLower())
                {
                    var user = subWeb.EnsureUser(_userName);
                    context.Load(user);
                    context.ExecuteQuery();

                    if (null != user)
                    {
                        ClientResult<BasePermissions> permissions = subWeb.GetUserEffectivePermissions(user.LoginName);
                        context.ExecuteQuery();


                        if (permissions.Value.Has(PermissionKind.ViewListItems))
                        {
                            exist = true;
                            break;
                        }
                        else
                            exist = false;


                    }
                    else
                        exist = false;




                }
            }

            return exist;
        }

        public bool GetUserGroup(ProjectContext context, string groupName)
        {
            bool exist = false;

            context.Load(context.Web);

            //  context.Load(web.SiteUsers);
            context.ExecuteQuery();

            Web web = context.Web;

            IEnumerable<User> user = context.LoadQuery(web.SiteUsers.Where(p => p.Email == _userName));
            context.ExecuteQuery();

            if (user.Any())
            {
                User userLogged = user.FirstOrDefault();

                context.Load(userLogged.Groups);
                context.ExecuteQuery();

                GroupCollection group = userLogged.Groups;

                IEnumerable<Group> usergroup = context.LoadQuery(userLogged.Groups.Where(p => p.Title == groupName));
                context.ExecuteQuery();
                if (!usergroup.Any())
                {
                    exist = false;
                }
                else
                    exist = true;
            }

            return exist;
        }

        private static PublishedProject GetProjectByName(string name, ProjectContext context)
        {
            if (name.Contains(" - "))
                name = name.Replace(" - ", "-");
            IEnumerable<PublishedProject> projs = context.LoadQuery(context.Projects.Where(p => p.Name == name));
            context.ExecuteQuery();
            if (!projs.Any())       // no project found
            {
                return null;
            }
            return projs.FirstOrDefault();

        }

        private static Web GetProjectWEB(string siteurl, ProjectContext context)
        {
            IEnumerable<Web> webs = context.LoadQuery(context.Web.Webs.Where(p => p.Url == siteurl));
            context.ExecuteQuery();
            if (!webs.Any())       // no project found
            {
                return null;
            }
            return webs.FirstOrDefault();

        }

      
      

        public string GetResourceAssignments(string ResourceName)
        {
            var markdownContent = "";
            int counttotalAss = 0;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {


                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);


                context.Load(context.Projects);
                context.ExecuteQuery();
                ProjectCollection projcoll = context.Projects;


                context.Load(context.EnterpriseResources);
                var resources = context.EnterpriseResources;

                context.ExecuteQuery();


                ResourceName = ResourceName.Replace(" ", String.Empty);

                string fullEmail = string.Concat(ResourceName, ConfigurationManager.AppSettings["DomainEmail"]);

                var user = context.Web.EnsureUser(ResourceName);
                context.Load(user);
                context.ExecuteQuery();

                if (user != null)
                {
                    var resource = resources.FirstOrDefault(i => i.Email == user.Email);
                    if (resource != null)
                    {

                        foreach (PublishedProject proj in projcoll)
                        {
                            context.Load(proj.Assignments, da => da.Where(a => a.Resource.Email == user.Email));

                            context.ExecuteQuery();

                            if (proj.Assignments != null)
                            {
                                PublishedAssignmentCollection proAssignment = proj.Assignments;
                                foreach (PublishedAssignment ass in proAssignment)
                                {
                                    context.Load(ass.Task);
                                    context.ExecuteQuery();
                                    var tsk = ass.Task;
                                    markdownContent += "**Project Name :**\n" + proj.Name + "<br>";
                                    markdownContent += "**Assignment Start Date :**\n" + ass.Start + "<br>";
                                    markdownContent += "**Task Name :**\n" + tsk.Name + "<br>";
                                    markdownContent += "----\n\n";
                                    counttotalAss++;
                                }

                            }
                        }

                        if (counttotalAss > 0)
                        {
                            markdownContent += "**Total Assignes :**\n" + counttotalAss + "<br>";
                        }
                        else
                        {
                            markdownContent = "No assigned task for this resource";
                        }
                    }
                    else
                    {
                        markdownContent = "Resource not found.";
                    }
                }
                else
                {
                    markdownContent = "Resource not found.";
                }



                return markdownContent;
            }

        }

        public IMessageActivity GetResourceLoggedInTasks(IDialogContext dialogContext, ProjectContext context, PublishedProject proj)
        {
            var SubtitleVal = "";
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            context.Load(proj.Assignments, da => da.Where(a => a.Resource.Email != string.Empty && a.Resource.Email == _userName));
            context.ExecuteQuery();
            if (proj.Assignments != null)
            {
                PublishedAssignmentCollection proAssignment = proj.Assignments;
                foreach (PublishedAssignment ass in proAssignment)
                {
                    context.Load(ass.Task);
                    context.Load(ass.Resource);

                    context.ExecuteQuery();
                    var tsk = ass.Task;
                    string TaskName = tsk.Name;
                    string TaskDuration = tsk.Duration;
                    string TaskPercentCompleted = tsk.PercentComplete.ToString();
                    string TaskStartDate = tsk.Start.ToString();
                    string TaskFinishDate = tsk.Finish.ToString();




                    SubtitleVal += "**Task Name**\n" + TaskName + "\n\r";
                    SubtitleVal += "**Task Duration**\n" + TaskDuration + "\n\r";
                    SubtitleVal += "**Task Percent Completed**\n" + TaskPercentCompleted + "\n\r";
                    SubtitleVal += "**Task Start Date**\n" + TaskStartDate + "\n\r";
                    SubtitleVal += "**Task Finish Date**\n" + TaskFinishDate + "\n\r";

                    HeroCard plCard = new HeroCard()
                    {
                        Title = TaskName,
                        Subtitle = SubtitleVal,

                    };

                    Microsoft.Bot.Connector.Attachment attachment = new Microsoft.Bot.Connector.Attachment()
                    {
                        ContentType = HeroCard.ContentType,
                        Content = plCard
                    };


                    reply.Attachments.Add(attachment);

                }
                HeroCard plCardCounter = new HeroCard()
                { Title = "**Total Tasks :**\n" + proAssignment.Count, };
                reply.Attachments.Add(plCardCounter.ToAttachment());


            }
            else
            {
                HeroCard plCardNoData = new HeroCard()
                { Title = "**No assigned task for you on this project**\n" };
                reply.Attachments.Add(plCardNoData.ToAttachment());

            }

            return reply;


        }

        public string getresourceassignedrisksIssues(IDialogContext dialogContext, ListItemCollection itemsIssue)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
            string SubtitleVal = "";
            string markdownContent = string.Empty;
            string IssueName = string.Empty;
            string IssueStatus = string.Empty;
            string IssuePriority = string.Empty;


            if (itemsIssue.Count > 0)
            {
                int count = 0;
                foreach (ListItem item in itemsIssue)
                {
                    if (item["AssignedTo"] != null)
                    {
                        count++;
                        FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];
                        if (fuv.Email == _userName)
                        {

                            if (item["Title"] != null)
                                IssueName = (string)item["Title"];
                            if (item["Status"] != null)
                                IssueStatus = (string)item["Status"];
                            if (item["Priority"] != null)
                                IssuePriority = (string)item["Priority"];
                            markdownContent += "**Title**\n" + IssueName + "<br>";
                            markdownContent += "**Status**\n" + IssueStatus + "\n\r";
                            markdownContent += "**Priority**\n" + IssuePriority + "\n\r";
                            markdownContent += "----\n\n";
                            //  if (item["Title"] != null)
                            //      IssueName = (string)item["Title"];
                            //  if (item["Status"] != null)
                            //      IssueStatus = (string)item["Status"];
                            //  if (item["Priority"] != null)
                            //      IssuePriority = (string)item["Priority"];
                            ////  SubtitleVal += "**Title**\n" + IssueName + "<br>";
                            //  SubtitleVal += "**Status**\n" + IssueStatus + "\n\r";
                            //  SubtitleVal += "**Priority**\n" + IssuePriority + "\n\r";




                            //HeroCard plCard = new HeroCard()
                            //{
                            //    Title = IssueName,
                            //    Subtitle = SubtitleVal,
                            //};

                            //Microsoft.Bot.Connector.Attachment attachment = new Microsoft.Bot.Connector.Attachment()
                            //{
                            //    ContentType = HeroCard.ContentType,
                            //    Content = plCard
                            //};

                            //reply.Attachments.Add(attachment);
                        }


                    }
                }
                //HeroCard plCardCounter = new HeroCard()
                //{ Title = "**Total Issues :**\n" + count, };
                //reply.Attachments.Add(plCardCounter.ToAttachment());
                markdownContent += "**Total Issues :**\n" + count + "\n\r";


            }
            else
            {
                //HeroCard plCardNoData = new HeroCard()
                //{ Title = "**No Issues assigned to you on this project**\n\n" };
                //reply.Attachments.Add(plCardNoData.ToAttachment());
                markdownContent += "**No Issues assigned to you on this project**\n\n";

            }

            return markdownContent;
        }
    }
}
