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

        public ProjectServer(string userName , string password)
        {
            _userName = userName;
            _userPassword = password;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }


        public string GetAllProjects(bool showCompletion , bool ProjectDates , bool ProjectDuration)
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
                    if (showCompletion == false && ProjectDates== false && ProjectDuration == false)
                    {
                        markdownContent += "**Project Name**\n\n" + "<br>";
                        foreach (PublishedProject pro in projectDetails)
                        {
                            markdownContent += pro.Name + "<br>";
                        }
                        markdownContent += "**Total Projects :**\n" + projectDetails.Count + "<br>";
                    }
                    else
                    {
                        foreach (PublishedProject pro in projectDetails)
                        {
                            markdownContent += "**Project Name**\n" + pro.Name + "<br>";

                            if (showCompletion==true)
                                markdownContent += "**Completed Percentage**\n" + pro.PercentComplete + "%<br/>";

                            if (ProjectDates == true)
                            {
                                markdownContent += "**Start Date**\n" + pro.StartDate + "<br>";
                                markdownContent += "**Finish Date**\n" + pro.FinishDate + "<br>";
                            }

                            if (ProjectDuration == true)
                            {
                                TimeSpan duration = pro.FinishDate - pro.StartDate;
                                markdownContent += "**Project Duration**\n" + duration.Days + "<br>";
                            }

                            markdownContent += "----\n\n";



                        }
                        markdownContent += "**Total Projects :**\n" + projectDetails.Count + "<br>";


                    }

                   
                }
                else
                    markdownContent += "**No Availabel Projects**\n\n";


            }         

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
                    if (ListName == Enums.ListName.Tasks.ToString())
                    {
                        markdownContent = GetProjectTasks(context, project);
                    }
                    else
                    {
                        Web projectweb = SubSiteExists(_siteUri, pName, context);
                        if (projectweb != null)
                        {
                            if (UserHavePermissionOnaProjects(_siteUri, pName, context))
                            {
                               if(ListName== Common.Enums.ListName.Issues.ToString())
                                    markdownContent = GetProjectIssues(projectweb);

                                if (ListName == Common.Enums.ListName.Risks.ToString())
                                    markdownContent = GetProjectRisks(projectweb);
                            }
                            else
                            {
                                markdownContent = "Sorry , You don't have access to this project";
                            }
                        }
                        else
                        {
                            markdownContent = "Site Project Not Created";
                        }
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
            var issues = projectweb.Lists.GetByTitle(Enums.ListName.Issues.ToString());
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection itemsIssue = issues.GetItems(query);

            projectweb.Context.Load(issues);
            projectweb.Context.Load(itemsIssue);
            projectweb.Context.ExecuteQuery();
       
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
                    markdownContent += "**Title**\n" + IssueName + "<br>";
                    markdownContent += "**Status**\n" + IssueStatus + "<br/>";
                    markdownContent += "**Priority**\n" + IssuePriority + "<br>";
                    markdownContent += "----\n\n";
                }

                markdownContent += "**Total Issues :**\n" + itemsIssue.Count + "<br>";
            }
            else
                markdownContent = "No Issies for this Project";

            return markdownContent;
        }

        public string GetProjectRisks(Web projectweb)
        {
            var markdownContent = "";
            string RiskName = string.Empty;
            string ResourceName = string.Empty;
            string riskStatus = string.Empty;

            var risks = projectweb.Lists.GetByTitle(Enums.ListName.Risks.ToString());
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection itemsRisk = risks.GetItems(query);

            projectweb.Context.Load(risks);
            projectweb.Context.Load(itemsRisk);
            projectweb.Context.ExecuteQuery();

            if (itemsRisk.Count > 0)
            {
                foreach (ListItem item in itemsRisk)
                {
                    if (item["Title"] != null)
                        RiskName = (string)item["Title"];
                    markdownContent += "**Risk Title**\n" + RiskName + "<br>";

                    if (item["AssignedTo"] != null)
                    {
                        FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];
                        markdownContent += "**Assigned To Resource**\n" + fuv.LookupValue + "<br/>";

                    }

                    if (item["Status"] != null)
                        riskStatus = (string)item["Status"];
                    markdownContent += "**Risk Status**\n" + riskStatus + "<br>";


                    markdownContent += "----\n\n";
                }

                markdownContent += "**Total Risks :**\n" + itemsRisk.Count + "<br>";
            }
            else
                markdownContent = "No Risks for this Project";

            return markdownContent;
        }

        public string GetProjectTasks(ProjectContext context, PublishedProject project)
        {
            var markdownContent = "";

            context.Load(project.Tasks);
            context.ExecuteQuery();
            PublishedTaskCollection tskcoll = project.Tasks;
            if (tskcoll.Count > 0)
            {
                foreach (PublishedTask tsk in tskcoll)
                {
                    string TaskName = tsk.Name;
                    string TaskDuration = tsk.Duration;
                    string TaskPercentCompleted = tsk.PercentComplete.ToString();
                    string TaskStartDate = tsk.Start.ToString();
                    string TaskFinishDate = tsk.Finish.ToString();


                    markdownContent += "**Task Name**\n" + TaskName + "<br>";
                    markdownContent += "**Task Duration**\n" + TaskDuration + "<br/>";
                    markdownContent += "**Task Percent Completed**\n" + TaskPercentCompleted + "<br>";
                    markdownContent += "**Task Start Date**\n" + TaskStartDate + "<br>";
                    markdownContent += "**Task Finish Date**\n" + TaskFinishDate + "<br>";
                    markdownContent += "----\n\n";
                }
                markdownContent += "**Total Tasks :**\n" + tskcoll.Count + "<br>";
            }
            else
                markdownContent = "No Tasks for this Project";

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

        public string GetProjectInfo(string pName, bool optionalDate = false, bool optionalDuration = false, bool optionalCompletion = false)
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

                if (project != null)
                {
                    if (optionalDate == true)
                    {
                        markdownContent += "**Start Date**\n" + project.StartDate + "<br>";
                        markdownContent += "**Finish Date**\n" + project.FinishDate + "<br>";
                    }

                    if (optionalDuration == true)
                    {
                        TimeSpan duration = project.FinishDate - project.StartDate;
                        markdownContent += "**Project Duration**\n" + duration.Days + "<br>";
                    }

                    if (optionalCompletion == true)
                        markdownContent += "**Project Completed Percentage**\n" + project.PercentComplete + "%<br>";
                }
                else
                {
                    markdownContent = "Project Name Not Exist";

                }



            }
            return markdownContent;
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
