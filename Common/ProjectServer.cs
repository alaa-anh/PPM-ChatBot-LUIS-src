﻿using System;
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


        public string GetAllProjects(bool showCompletion , bool ProjectDates , bool ProjectDuration , bool projectManager)
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
                    if (showCompletion == false && ProjectDates== false && ProjectDuration == false && projectManager == false)
                    {
                     //   markdownContent += "**Project Name**\n\n" + "<br>";
                        foreach (PublishedProject pro in projectDetails)
                        {
                            markdownContent += "**Project Name**\n" + pro.Name + "<br>";
                            markdownContent += "**Completed Percentage**\n" + pro.PercentComplete + "%<br/>";
                            markdownContent += "**Start Date**\n" + pro.StartDate + "<br>";
                            markdownContent += "**Finish Date**\n" + pro.FinishDate + "<br>";
                            TimeSpan duration = pro.FinishDate - pro.StartDate;
                            markdownContent += "**Project Duration**\n" + duration.Days + "<br>";
                            context.Load(pro.Owner);
                            context.ExecuteQuery();
                               markdownContent += "**Project Manager**\n" + pro.Owner.Title + "<br>";

                            markdownContent += "----\n\n";
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


                            if (projectManager == true)
                            {
                                context.Load(pro.Owner);
                                context.ExecuteQuery();
                                markdownContent += "**Project Manager**\n" + pro.Owner.Title + "<br>";
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
                         
                PublishedProject project = GetProjectByName(pName, context);
                
                if(project !=null)
                {
                    if (ListName == Enums.ListName.Tasks.ToString())
                    {
                        markdownContent = GetProjectTasks(context, project);
                    }
                   else if (ListName == Enums.ListName.Assignments.ToString())
                    {
                        markdownContent = GetProjectTAssignments(context, project);
                    }
                    else
                    {
                        Web projectweb = SubSiteExists(_siteUri, pName);
                        if (projectweb != null)
                        {
                           // if (UserHavePermissionOnaProjects(_siteUri, pName, context))
                          //  {
                               if(ListName== Common.Enums.ListName.Issues.ToString())
                                    markdownContent = GetProjectIssues(projectweb , context);

                                if (ListName == Common.Enums.ListName.Risks.ToString())
                                    markdownContent = GetProjectRisks(projectweb , context);

                                if (ListName == Common.Enums.ListName.Deliverables.ToString())
                                    markdownContent = GetProjectDeliverables(projectweb , context);
                            //}
                            //else
                            //{
                            //    markdownContent = "Sorry , You don't have access to this project";
                            //}
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
            return markdownContent;
        }

        public string GetProjectIssues(Web projectweb, ProjectContext context)
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

            if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Web Administrators (Project Web App Synchronized)"))
            {
                markdownContent = GetAllProjectIssues(itemsIssue);
            }
            else if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
            {
                markdownContent = getresourceassignedrisksIssues(itemsIssue);
            }

            return markdownContent;
        }


        public string GetAllProjectIssues(ListItemCollection itemsIssue)
        {
            var markdownContent = "";
            string IssueName = string.Empty;
            string IssueStatus = string.Empty;
            string IssuePriority = string.Empty;
           

            if (itemsIssue.Count > 0)
            {
                foreach (ListItem item in itemsIssue)
                {
                    if (item["Title"] != null)
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

        public string getresourceassignedrisksIssues(ListItemCollection itemsIssue)
        {
            var markdownContent = "";
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
                            markdownContent += "**Status**\n" + IssueStatus + "<br/>";
                            markdownContent += "**Priority**\n" + IssuePriority + "<br>";
                            markdownContent += "----\n\n";
                        }
                    }
                }

                markdownContent += "**Total Issues :**\n" + count + "<br>";
            }
            else
                markdownContent = "No Issues assigned to you on this project";

            return markdownContent;
        }

        public string GetProjectRisks(Web projectweb , ProjectContext context)
        {
            var markdownContent = "";
           



            var risks = projectweb.Lists.GetByTitle(Enums.ListName.Risks.ToString());
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection itemsRisk = risks.GetItems(query);

            projectweb.Context.Load(risks);
            projectweb.Context.Load(itemsRisk);
            projectweb.Context.ExecuteQuery();
            if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Web Administrators (Project Web App Synchronized)"))
            {
                markdownContent = getallrisks(itemsRisk);
            }
            else if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
            {
                markdownContent = getresourceassignedrisks(itemsRisk);
            }
            

        

            return markdownContent;
        }

        public string getallrisks(ListItemCollection itemsRisk)
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
                    else
                        markdownContent += "**Assigned To Resource :**\n" + "Not assigned"+ "<br/>";

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

                markdownContent += "**Total Risks :**\n" + itemsRisk.Count + "<br>";
            }
            else
                markdownContent = "No Risks for this Project";

            return markdownContent;
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
        public string GetProjectDeliverables(Web projectweb, ProjectContext context)
        {
            var markdownContent = "";
            string DeliverableName = string.Empty;
            string DeliverableStart = string.Empty;
            string DeliverableFinish = string.Empty;

            if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
            {
                markdownContent = "You Don't have permission to view the deliverables of this project";

            }
            else
            {
                var delive = projectweb.Lists.GetByTitle(Enums.ListName.Deliverables.ToString());
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsdelive = delive.GetItems(query);

                projectweb.Context.Load(delive);
                projectweb.Context.Load(itemsdelive);
                projectweb.Context.ExecuteQuery();

                if (itemsdelive.Count > 0)
                {
                    foreach (ListItem item in itemsdelive)
                    {
                        if (item["Title"] != null)
                            DeliverableName = (string)item["Title"];
                        markdownContent += "**Deliverable Name**\n" + DeliverableName + "<br>";

                        if (item["Author"] != null)
                        {
                            FieldUserValue fuv = (FieldUserValue)item["Author"];
                            markdownContent += "**Create By Resource :**\n" + fuv.LookupValue + "<br/>";

                        }

                        if (item["CommitmentStart"] != null)
                            DeliverableStart = item["CommitmentStart"].ToString();
                        markdownContent += "**Start Date :**\n" + DeliverableStart + "<br>";

                        if (item["CommitmentFinish"] != null)
                            DeliverableFinish = item["CommitmentFinish"].ToString();
                        markdownContent += "**Finish Date :**\n" + DeliverableFinish + "<br>";

                        markdownContent += "----\n\n";
                    }

                    markdownContent += "**Total Deliverabels :**\n" + itemsdelive.Count + "<br>";
                }
                else
                    markdownContent = "No Deliverabels for this Project";
            }

            return markdownContent;
        }

        public string GetProjectTasks(ProjectContext context, PublishedProject project)
        {
            var markdownContent = "";


            if (GetUserGroup(context, "Project Managers (Project Web App Synchronized)") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Portfolio Managers for Project Web App") || GetUserGroup(context, "Web Administrators (Project Web App Synchronized)"))
            {
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
            }

            else
            {
                if (GetUserGroup(context, "Team Members (Project Web App Synchronized)"))
                    markdownContent = GetResourceTasksLoggedIn(context , project);
            }
            return markdownContent;
        }
        public string GetProjectTAssignments(ProjectContext context, PublishedProject project)
        {
            var markdownContent = "";

            context.Load(project.Assignments);
            context.ExecuteQuery();
            PublishedAssignmentCollection asscoll = project.Assignments;
            if (asscoll.Count > 0)
            {
                foreach (PublishedAssignment ass in asscoll)
                {
                    context.Load(ass.Task);
                    context.Load(ass.Resource);
                    context.ExecuteQuery();
                    markdownContent += "**Task Name :**\n" +ass.Task.Name + "<br>";
                    markdownContent += "**Resource Name :**\n" + ass.Resource.Name + "<br>";
                    markdownContent += "**Start Date**\n" + ass.Start + "<br>";
                    markdownContent += "**Finish Date**\n" + ass.Finish + "<br>";
                    markdownContent += "----\n\n";
                }
                markdownContent += "**Total Assignments :**\n" + asscoll.Count + "<br>";
            }
            else
                markdownContent = "No Assignments for this Project";

            return markdownContent;
        }

        public Web SubSiteExists(string siteUrl, string subSiteTitle)
        {
            Web projectweb = null;
            using (ClientContext context = new ClientContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in ConfigurationManager.AppSettings["DomainAdminPassword"].ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(ConfigurationManager.AppSettings["DomainAdmin"], passWord);

                Web oWebsite = context.Web;
                context.Load(oWebsite);
                context.ExecuteQuery();

                  context.Load(oWebsite.Webs);
                   context.ExecuteQuery();

                foreach(Web w in oWebsite.Webs)
                {
                    if(w.Title.ToLower() == subSiteTitle.ToLower())
                    {
                        projectweb = w;
                        break;
                    }
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

        public bool GetUserGroup(ProjectContext context , string groupName)
        {
            bool exist = false;

            Web web = context.Web;
            IEnumerable<User> user = context.LoadQuery(web.SiteUsers.Where(p => p.Email == _userName));
            context.ExecuteQuery();

            User userLogged = user.FirstOrDefault();

            context.Load(userLogged.Groups);
            context.ExecuteQuery();

            GroupCollection group = userLogged.Groups;

            IEnumerable<Group> usergroup  = context.LoadQuery(userLogged.Groups.Where(p => p.Title == groupName));
            context.ExecuteQuery();
            if (!usergroup.Any())       
            {
                exist = false;
            }
            else
                exist = true;
            return exist;
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

        public string GetProjectInfo(string pName, bool optionalDate = false, bool optionalDuration = false, bool optionalCompletion = false , bool optionalPM = false)
        {
            var markdownContent = "";

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
                        markdownContent += "**Start Date :**\n" + project.StartDate + "<br>";
                        markdownContent += "**Finish Date :**\n" + project.FinishDate + "<br>";
                    }

                    if (optionalDuration == true)
                    {
                        TimeSpan duration = project.FinishDate - project.StartDate;
                        markdownContent += "**Project Duration :**\n" + duration.Days + "<br>";
                    }

                    if (optionalCompletion == true)
                        markdownContent += "**Project Completed Percentage :**\n" + project.PercentComplete + "%<br>";

                    if (optionalPM == true)
                    {
                        if (GetUserGroup(context, "Team Members (Project Web App Synchronized)") == false)
                        {
                            context.Load(project.Owner);
                            context.ExecuteQuery();
                            markdownContent += "**Project Manager Name :**\n" + project.Owner.Title + "<br>";
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
              
        public string FilterProjectsByDate(string FilterType, string pStartDate, string PEndDate, string ProjectSEdateFlag)
        {
            string markdownContent = string.Empty;
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

            }
            if (!retrivedProjects.Any())       // no project found
            {
                markdownContent += "**No Availabel Projects**\n\n";

               // return null;
            }
            else
            {
                foreach (var item in retrivedProjects)
                {
                    markdownContent += "**Project Name**\n" + item.Name + "<br>";
                    markdownContent += "**Start Date**\n" + item.StartDate + "<br>";
                    markdownContent += "**Finish Date**\n" + item.FinishDate + "<br>";
                    markdownContent += "**Actual Cost**\n" + item.DefaultFixedCostAccrual.ToString() + "<br>";
                    markdownContent += "----\n\n";


                }
                markdownContent +="**Total Projects :**\n" + retrivedProjects.Count() + "<br>";

            }

            return markdownContent;
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

                var user =context.Web.EnsureUser(ResourceName);
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

        public string GetResourceTasksLoggedIn(ProjectContext context , PublishedProject proj)
        {
            var markdownContent = "";           
            context.Load(proj.Assignments, da => da.Where(a => a.Resource.Email !=string.Empty && a.Resource.Email == _userName));
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


                    markdownContent += "**Task Name**\n" + TaskName + "<br>";
                    markdownContent += "**Task Duration**\n" + TaskDuration + "<br/>";
                    markdownContent += "**Task Percent Completed**\n" + TaskPercentCompleted + "<br>";
                    markdownContent += "**Task Start Date**\n" + TaskStartDate + "<br>";
                    markdownContent += "**Task Finish Date**\n" + TaskFinishDate + "<br>";
                    markdownContent += "----\n\n";

                }
                markdownContent += "**Total Assignes :**\n" + proAssignment.Count + "<br>";

            }            
            else
            {
                markdownContent = "No assigned task for you on this project";
            }

            return markdownContent;
            

        }
    }
}
