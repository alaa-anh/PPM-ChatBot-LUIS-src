using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using Common.Contracts;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;


namespace Common
{
    public class ProjectServer
    {
        private string _userName;
        private string _userPassword;

        private string _siteUri;



        public ProjectServer(string userName)
        {
            _userName = userName;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }

        public ProjectServer(string userName , string password)
        {
            _userName = userName;
            _userPassword = password;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }


        public string GetAllProjects(bool showCompletion)
        {
            var markdownContent = "";
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Load(context.Projects);
                context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;
                if (context.Projects.Count > 0)
                {
                    if (showCompletion == true)
                    {
                        foreach (PublishedProject pro in projectDetails)
                        {
                            markdownContent += "**Project Name**\n" + pro.Name + "<br>";
                            markdownContent += "**Completed Percentage**\n" + pro.PercentComplete + "%<br/>";

                        }
                    }
                    else
                    {
                        markdownContent += "**Project Name**\n\n" + "<br>";
                        foreach (PublishedProject pro in projectDetails)
                        {
                            markdownContent += pro.Name + "<br>";
                        }
                    }

                    markdownContent += "**Total Projects :**\n" + projectDetails.Count + "<br>";
                }
                else
                    markdownContent += "**No Availabel Projects**\n\n";


            }
            //markdownContent += "##A subheading\n";
            //markdownContent += "**something bold**\n\n";
            //markdownContent += "*something italic*\n\n";
            //markdownContent += "[a link!](http://robinosborne.co.uk/?s=bot)\n\n";
            //markdownContent += "![AN IMAGE!](http://robinosborne.co.uk/wp-content/uploads/2016/07/robinosborne.jpg)\n";
            //markdownContent += "> A quote of something interesting\n\n";
            //markdownContent += "```\nvar this = \"code\";\n```\n";

            return markdownContent;
        }

        public string GetProjectSubItems(string pName , string ListName)
        {
            var markdownContent = "";

            using (ProjectContext context = new ProjectContext(_siteUri))
            {
               

                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);

                context.Load(context.Projects);
                context.Load(context.Web);
                context.ExecuteQuery();

                PublishedProject project = GetProjectByName(pName, context);
                if(project !=null)
                {
                    Web projectweb = SubSiteExists(_siteUri, pName, context);
                    if (projectweb !=null)
                    {
                        if (UserHavePermissionOnaProjects(_siteUri, pName, context))
                        {
                            markdownContent = GetProjectIssues(projectweb);
                           
                           
                        }
                        else
                        {
                            markdownContent = "Sorry , You don't have access to this project";
                        }
                    }
                    else
                    {
                        markdownContent = "No Issies for this Project";
                    }
                }
                else
                {
                    markdownContent = "Project Name Not Exist";
                }
               
            }
            return markdownContent;
        }

        public string GetProjectIssues(Web projectweb)
        {
            var markdownContent = "";
            string IssueName = string.Empty;
            string IssueStatus = string.Empty;
            string IssuePriority = string.Empty;
            var issues = projectweb.Lists.GetByTitle("Issues");
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection itemsIssue = issues.GetItems(query);

            projectweb.Context.Load(issues);
            projectweb.Context.Load(itemsIssue);
            projectweb.Context.ExecuteQuery();
       //     markdownContent += "|Title|Status|Priority|";
          //  markdownContent += "|---|---|---:|";
            if (itemsIssue.Count > 0)
            {
                foreach (ListItem item in itemsIssue)
                {
                    if (item["Title"] !=null)
                        IssueName = (string)item["Title"];
                    if (item["Status"] != null)
                        IssueStatus = (string)item["Status"];
                    if (item["Priority"] != null)
                        IssuePriority = (string)item["Priority"];

              //      markdownContent += "|"+ IssueName + "|"+ IssueStatus + "|"+ IssuePriority + "|";


                   // markdownContent += "**Title**\n" + IssueName + "<br>";
                   // markdownContent += "**Status**\n" + IssueStatus + "<br/>";
                   // markdownContent += "**Priority**\n" + IssuePriority + "<br/>";
                }

                markdownContent += "**Total Issues :**\n" + itemsIssue.Count + "<br>";
            }
            else
                markdownContent = "No Issies for this Project";

            markdownContent += "|Name|Value|";
            markdownContent += "| ---| ---:|";
            markdownContent += "| Status | Active |";
            markdownContent += "| Balance |£0.00 |";
            markdownContent += "| Credit Limit |£0.00 |";
            markdownContent += "| Available Credit |£0.00 |";
            return markdownContent;
        }

        public Web SubSiteExists(string siteUrl, string subSiteTitle, ClientContext context)
        {
            var web = context.Web;
            Web projectweb = null;
          //  bool exist = false;
            context.Load(web, w => w.Webs);
            context.ExecuteQuery();
            foreach(Web ww in web.Webs)
            {
                
                if (ww.Title.ToLower() == subSiteTitle.ToLower())
                {
                  //  exist = true;
                    projectweb = ww;
                    break;
                }
            }
           
            return projectweb;
        }

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

        public string FindProjectByName(string searchTermName)
        {
            string projects = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                CamlQuery query = new CamlQuery();


                context.Load(context.Projects);
                context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;
                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }

      

       


        private static PublishedProject GetProjectByName(string name, ProjectContext context)
        {
            IEnumerable<PublishedProject> projs = context.LoadQuery(context.Projects.Where(p => p.Name == name));
            context.ExecuteQuery();
            
            if (!projs.Any())       // no project found
            {
                return null;
            }
            return projs.FirstOrDefault();
        }

       

        public string GetProjectTasks(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);


                context.Load(project.Tasks);
                context.ExecuteQuery();
                PublishedTaskCollection tskcoll = project.Tasks;


                foreach (PublishedTask tsk in tskcoll)
                {
                    //tsk.ActualStart = Convert.ToDateTime("ActualStart");
                    //tsk.ActualFinish = Convert.ToDateTime("ActualFinish");
                    //tsk.Work = "Work";
                    //tsk.ActualWork = "ActualWork";
                    //tsk.PercentComplete = Convert.ToInt32("PercentComplete");


                    string TaskName = tsk.Name;
                    string TaskDuration = tsk.Duration;
                    string TaskPercentCompleted = tsk.PercentComplete.ToString();
                    string TaskStartDate = tsk.Start.ToString();
                    string TaskFinishDate = tsk.Finish.ToString();
                    strissues = strissues + TaskName + "," + TaskDuration + "," + TaskPercentCompleted + "," + TaskStartDate + "," + TaskFinishDate + "<br>";


                }



            }
            return strissues;
        }


        public string GetProjectInfo(string pName, bool optionalDate = false, bool optionalDuration = false, bool optionalCompletion = false)
        {
            string strissues = string.Empty;
            Token token = new Mongo().Get<Token>("ContextTokens", "UserName", this._userName);

            using (ClientContext contextah = TokenHelper.GetClientContextWithContextToken(_siteUri, token.ContextToken, _siteUri))
            {
                using (ProjectContext context = new ProjectContext(_siteUri))
                {
                    //SecureString passWord = new SecureString();
                    //foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                    //context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                    PublishedProject project = GetProjectByName(pName, context);

                    if (optionalDate == true)
                        strissues = strissues + project.StartDate + "," + project.FinishDate + ",";

                    if (optionalDuration == true)
                    {
                        TimeSpan duration = project.FinishDate - project.StartDate;
                        strissues = strissues + duration.Days + ",";
                    }

                    if (optionalCompletion == true)
                        strissues = strissues + project.PercentComplete + "%" + ",";



                }
            }
            return strissues;
        }

        public string GetProjectRiskResources(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);

                context.Load(context.Web);
                context.ExecuteQuery();

                var issues = context.Web.Lists.GetByTitle("Risks");
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsIssue = issues.GetItems(query);

                context.Load(issues);
                context.Load(itemsIssue);
                context.ExecuteQuery();


                foreach (ListItem item in itemsIssue)
                {
                    string IssueName = (string)item["Title"];


                    FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];



                    strissues = strissues + IssueName + "," + "," + fuv.LookupValue + "<br>";

                }

            }
            return strissues;
        }

        public string GetProjectRiskStatus(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);

                context.Load(context.Web);
                context.ExecuteQuery();

                var issues = context.Web.Lists.GetByTitle("Risks");
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsIssue = issues.GetItems(query);

                context.Load(issues);
                context.Load(itemsIssue);
                context.ExecuteQuery();


                foreach (ListItem item in itemsIssue)
                {
                    string IssueName = (string)item["Title"];
                    string riskStatus = (string)item["Status"];
                    strissues = strissues + IssueName + "," + "," + riskStatus + "<br>";
                }

            }
            return strissues;
        }

        public string FilterProjectsByDate(string FilterType, string pStartDate, string PEndDate, string ProjectSEdateFlag)
        {
            string strissues = string.Empty;
            IEnumerable<PublishedProject> retrivedProjects = null; ;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
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

            }
            if (!retrivedProjects.Any())       // no project found
            {
                return null;
            }
            else
            {
                foreach (var item in retrivedProjects)
                {
                    strissues = strissues + item.Name + "," + item.StartDate + "," + item.FinishDate + "," + item.DefaultFixedCostAccrual.ToString() + "<br>";

                }
            }

            return strissues;
        }
    }
}
