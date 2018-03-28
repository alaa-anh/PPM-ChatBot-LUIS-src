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
    public class WebChat
    {
        private string _userName;
        private string _userPassword;

        private string _siteUri;
        public WebChat(string userName, string password)
        {
            _userName = userName;
            _userPassword = password;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }


        public IMessageActivity GetAllProjects(IDialogContext dialogContext, int SIndex, bool showCompletion, bool ProjectDates, bool PDuration, bool projectManager, out int Counter)
        {
            IMessageActivity reply = null;
            reply = dialogContext.MakeMessage();
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


                        HeroCard plCard = new HeroCard()
                        {
                            Title = ProjectName,
                            Subtitle = SubtitleVal,
                            Images = cardImages,
                            Buttons = cardactions,
                            Tap = btnTasks,

                        };

                        Microsoft.Bot.Connector.Attachment attachment = new Microsoft.Bot.Connector.Attachment()
                        {
                            ContentType = HeroCard.ContentType,
                            Content = plCard
                        };
                        reply.Attachments.Add(attachment);
                    }


                }

            }

            return reply;
        }


    }
}
