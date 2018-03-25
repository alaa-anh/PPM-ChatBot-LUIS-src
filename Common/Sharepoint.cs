using System.Linq;
using System.Security;
using Microsoft.SharePoint.Client;


//using Common.Contracts;


namespace Common
{
    public class Sharepoint
    {
        private string _userName = "Alaa@M365x671106.onmicrosoft.com";//"admin @M365x671106.onmicrosoft.com";
        private string _siteUri = "https://m365x671106.sharepoint.com/sites/pwa/";

        public Sharepoint(string userName)
        {
            _userName = "Alaa@M365x671106.onmicrosoft.com";
            _siteUri = "https://m365x671106.sharepoint.com/sites/pwa/";// ConfigurationManager.AppSettings["SHAREPOINT_SITE_URI"];
        }

        public string FindUsersByName(string searchTermName)
        {
            string users = string.Empty;


            //Token token = new Mongo().Get<Token>("ContextTokens", "UserName", this._userName);

            //using (ClientContext context = TokenHelper.c(_siteUri, token.ContextToken, "localhost:44335"))
            //using (ClientContext context = TokenHelper.GetClientContextWithContextToken(_siteUri, "Alaa@M365x671106.onmicrosoft.com", "localhost:44335"))
            using ( ClientContext context = new ClientContext(_siteUri))
            {
                //context.Credentials = System.Net.CredentialCache.DefaultCredentials;
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);

                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);


                CamlQuery query = new CamlQuery();
                query.ViewXml = $"<View><Query><Where><Contains><FieldRef Name='Title' /><Value Type='Text'>{searchTermName}</Value></Contains></Where></Query></View>";
                // ListItemCollection peopleDetails = context.Web.Lists.GetByTitle("Users").GetItems(query);
                UserCollection peopleDetails = context.Web.SiteUsers;//.Lists.GetByTitle("Users").GetItems(query);
                context.Load(peopleDetails);
                context.ExecuteQuery();

                //users = string.Join("<br>", peopleDetails.Select(x => x["Title"] + "(" + x["ContactNumber"] + "),"));
                users = string.Join("<br>", peopleDetails.Select(x => x.LoginName + "(" + x.Email + "),"));
            }
            return users;
        }


        //public IMessageActivity GetAllProjects(IDialogContext context, bool showCompletion, bool ProjectDates, bool PDuration, bool projectManager)
        //{
        //    IMessageActivity reply = null;
        //    reply = context.MakeMessage();
        //    SecureString passWord = new SecureString();
        //    foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
        //    SharePointOnlineCredentials credentials = new SharePointOnlineCredentials(_userName, passWord);
        //    var webUri = new Uri(_siteUri);
        //    using (var client = new WebClient())
        //    {
        //        client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
        //        client.Credentials = credentials;
        //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
        //        client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
        //        var endpointUri = new Uri(webUri + "/_api/ProjectData/Projects");
        //        var responce = client.DownloadString(endpointUri);
        //        var t = JToken.Parse(responce);
        //        JObject results = JObject.Parse(t["d"].ToString());
        //        List<JToken> jArrays = ((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)t["d"]).First).First.ToList();
        //        foreach (var item in jArrays)
        //        {
        //            string ProjectName = (string)item["ProjectName"];
        //            string ProjectWorkspaceInternalUrl = (string)item["ProjectWorkspaceInternalUrl"];
        //            string ProjectPercentCompleted = (string)item["ProjectPercentCompleted"];
        //            string ProjectFinishDate = (string)item["ProjectFinishDate"];
        //            string ProjectStartDate = (string)item["ProjectStartDate"];
        //            string ProjectDuration = (string)item["ProjectDuration"];
        //            string ProjectOwnerName = (string)item["ProjectOwnerName"];
        //        //    List<JToken> jArraysTasks = ((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)item["Tasks"]).First).First.ToList();

        //         //   string Tasks = (string)jArraysTasks[0];


        //            string SubtitleVal = "";

        //            if (showCompletion == false && ProjectDates == false && PDuration == false && projectManager == false)
        //            {
        //                SubtitleVal += "**Completed Percentage :**\n" + ProjectPercentCompleted + "%\n\r";
        //                SubtitleVal += "**Start Date :**\n" + ProjectStartDate + "\n\r";
        //                SubtitleVal += "**Finish Date :**\n" + ProjectFinishDate + "\n\r";
        //                SubtitleVal += "**Project Duration :**\n" + ProjectDuration + "\n\r";
        //                SubtitleVal += "**Project Manager :**\n" + ProjectOwnerName + "\n\r";
        //            }

        //            else if (showCompletion == true)
        //                SubtitleVal += "**Completed Percentage :**\n" + ProjectPercentCompleted + "%\n\r";

        //            else if (ProjectDates == true)
        //            {
        //                SubtitleVal += "**Start Date :**\n" + ProjectStartDate + "\n\r";
        //                SubtitleVal += "**Finish Date :**\n" + ProjectFinishDate + "\n\r";
        //            }

        //           else if (PDuration == true)
        //            {
        //                SubtitleVal += "**Project Duration :**\n" + ProjectDuration + "\n\r";
        //            }


        //            else if (projectManager == true)
        //            {
        //                SubtitleVal += "**Project Manager :**\n" + ProjectOwnerName + "\n\r";
        //            }


        //            var actionsButton = new List<CardAction>();
        //            CardAction buttonProjectURL = new CardAction()
        //            {
        //                Value = ProjectWorkspaceInternalUrl,
        //                Type = ActionTypes.OpenUrl,
        //                //  Image = strlike,
        //                Title = "Project Site"

        //            };
        //            actionsButton.Add(buttonProjectURL);
        //           // GetProjectTasks(Tasks);

        //            //CardAction buttonIssues = new CardAction()
        //            //{
        //            //    Value = "Issues",
        //            //    Type = ActionTypes.PostBack,
        //            //  //  Image = strlike,
        //            //    Title = "Issues"
        //            //};
        //            //actionsButton.Add(buttonIssues);

        //            HeroCard plCard = new HeroCard()
        //            {
        //                Title = ProjectName,
        //                Subtitle = SubtitleVal,// $" Wikipedia \r\r Page \n\r test",
        //                                       //  Text = "line 1 \r\r line 2 \n\r line 3 \r line 4"

        //                //Title = ProjectName + $"I'm \r\r a hero \n\r card",
        //                //Subtitle = $" Wikipedia \r\r Page \n\r test",
        //                //Text = "line 1 \r\r line 2 \n\r line 3 \r line 4"
        //                 Buttons = actionsButton,

        //            };

        //            Microsoft.Bot.Connector.Attachment attachment = new Microsoft.Bot.Connector.Attachment()
        //            {
        //                ContentType = HeroCard.ContentType,
        //                Content = plCard
        //            };


        //            reply.Attachments.Add(attachment);
        //        }

        //        HeroCard plCard2 = new HeroCard()
        //        {
        //            Title = "Total Projects :" + jArrays.Count(),
        //            //  Text = "line 1 \r\r line 2 \n\r line 3 \r line 4"

        //            //Title = ProjectName + $"I'm \r\r a hero \n\r card",
        //            //Subtitle = $" Wikipedia \r\r Page \n\r test",
        //            //Text = "line 1 \r\r line 2 \n\r line 3 \r line 4"
        //        };

        //        reply.Attachments.Add(plCard2.ToAttachment());
        //        return reply;
        //    }
        //}

        //public Web SubSiteExists(string siteUrl)
        //{
        //    Web projectweb = null;
        //    using (ClientContext context = new ClientContext(_siteUri))
        //    {
        //        SecureString passWord = new SecureString();
        //        foreach (char c in ConfigurationManager.AppSettings["DomainAdminPassword"].ToCharArray()) passWord.AppendChar(c);
        //        context.Credentials = new SharePointOnlineCredentials(ConfigurationManager.AppSettings["DomainAdmin"], passWord);

        //        projectweb = context.Web;
        //        context.Load(projectweb);
        //        context.ExecuteQuery();

        //        //  context.Load(oWebsite.Webs);
        //        //   context.ExecuteQuery();

        //        //foreach(Web w in oWebsite.Webs)
        //        //{
        //        //    //if (w.Id == proID)
        //        //    //{
        //        //    //    projectweb = w;
        //        //    //    break;
        //        //    //}
        //        //    if (w.Title.ToLower() == subSiteTitle.ToLower())
        //        //    {
        //        //        projectweb = w;
        //        //        break;
        //        //    }
        //        //}

        //    }
        //    return projectweb;
        //}

        //public string GetProjectTasks(ProjectContext context, PublishedProject project)
        //{
        //    var markdownContent = "";

        //    if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
        //    {
        //        markdownContent = GetResourceTasksLoggedIn(context, project);
        //    }
        //    else
        //    {
        //        context.Load(project.Tasks);
        //        context.ExecuteQuery();
        //        PublishedTaskCollection tskcoll = project.Tasks;
        //        if (tskcoll.Count > 0)
        //        {
        //            foreach (PublishedTask tsk in tskcoll)
        //            {
        //                string TaskName = tsk.Name;
        //                string TaskDuration = tsk.Duration;
        //                string TaskPercentCompleted = tsk.PercentComplete.ToString();
        //                string TaskStartDate = tsk.Start.ToString();
        //                string TaskFinishDate = tsk.Finish.ToString();


        //                markdownContent += "**Task Name**\n" + TaskName + "<br>";
        //                markdownContent += "**Task Duration**\n" + TaskDuration + "<br/>";
        //                markdownContent += "**Task Percent Completed**\n" + TaskPercentCompleted + "<br>";
        //                markdownContent += "**Task Start Date**\n" + TaskStartDate + "<br>";
        //                markdownContent += "**Task Finish Date**\n" + TaskFinishDate + "<br>";
        //                markdownContent += "----\n\n";
        //            }
        //            markdownContent += "**Total Tasks :**\n" + tskcoll.Count + "<br>";
        //        }
        //        markdownContent += "No Tasks For This Projects";
        //    }



        //    return markdownContent;
        //}

        //public bool GetUserGroup(ProjectContext context, string groupName)
        //{
        //    bool exist = false;

        //    context.Load(context.Web);

        //    //  context.Load(web.SiteUsers);
        //    context.ExecuteQuery();

        //    Web web = context.Web;

        //    IEnumerable<User> user = context.LoadQuery(web.SiteUsers.Where(p => p.Email == _userName));
        //    context.ExecuteQuery();

        //    if (user.Any())
        //    {
        //        User userLogged = user.FirstOrDefault();

        //        context.Load(userLogged.Groups);
        //        context.ExecuteQuery();

        //        GroupCollection group = userLogged.Groups;

        //        IEnumerable<Group> usergroup = context.LoadQuery(userLogged.Groups.Where(p => p.Title == groupName));
        //        context.ExecuteQuery();
        //        if (!usergroup.Any())
        //        {
        //            exist = false;
        //        }
        //        else
        //            exist = true;
        //    }

        //    return exist;
        //}
    }
}
