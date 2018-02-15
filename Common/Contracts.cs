﻿using MongoDB.Bson;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts
{
    public class Token
    {
        public ObjectId _id;
        public string UserName;
        public string ContextToken;

      

        public Token(string userName, string contextToken)
        {
            UserName = userName;
            ContextToken = contextToken;
        }

        public Token(string userName)
        {
            UserName = userName;

        }
    }
}