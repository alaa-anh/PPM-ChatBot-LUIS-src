using System;

namespace Common.Contracts
{
    public class Token
    {
        public Object _id;
        public string UserName;
        public string ContextToken;

        public Token(string userName, string contextToken)
        {
            UserName = userName;
            ContextToken = contextToken;
        }
    }
}