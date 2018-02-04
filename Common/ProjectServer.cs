using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Microsoft.ProjectServer.Client;

namespace Common
{
    public class ProjectServer
    {
        private string _userName = "Alaa@M365x671106.onmicrosoft.com";//"admin @M365x671106.onmicrosoft.com";
        private string _siteUri = "https://m365x671106.sharepoint.com/sites/pwa/";

        public ProjectServer(string userName)
        {
            _userName = "Alaa@M365x671106.onmicrosoft.com";
            _siteUri = "https://m365x671106.sharepoint.com/sites/pwa/";// ConfigurationManager.AppSettings["SHAREPOINT_SITE_URI"];
        }

        public string FindProjectByName(string searchTermName)
        {
            string projects = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                CamlQuery query = new CamlQuery();


                context.Load(context.Projects);
                context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;


                //projects = string.Join("<br>", projectDetails.Select(x => x.Name + "(" + x.ProjectSiteUrl + "),"));
                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }
    }
}
