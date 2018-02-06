using System.Linq;
using System.Security;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;


namespace Common
{
    public class ProjectServer
    {
        private string _userName;
        private string _siteUri;



        public ProjectServer(string userName, string ppmsiteurl)
        {
            _userName = "Alaa@M365x671106.onmicrosoft.com";
            _siteUri = ppmsiteurl;
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
                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }

        public string GetAllProjects()
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
                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }


        public string GetProjectIssues(string pName)
        {


            string projects = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                context.Load(context.Projects);
                context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;



                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }

    }
}
