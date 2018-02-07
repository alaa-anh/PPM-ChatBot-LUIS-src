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
    }
}
