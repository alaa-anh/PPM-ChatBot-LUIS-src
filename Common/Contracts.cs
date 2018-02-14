
using System;

namespace Common.Contracts
{
    public class Token
    {
        public Object _id;
        public string UserName;
        public string ContextToken;

        //public const string PersistedCookieKey = "persistedCookie";
        //public const string AuthResultKey = "authResult";
        //public const string MagicNumberKey = "authMagicNumber";
        //public const string MagicNumberValidated = "authMagicNumberValidated";

        public Token(string userName, string contextToken)
        {
            UserName = userName;
            ContextToken = contextToken;
        }
    }
}