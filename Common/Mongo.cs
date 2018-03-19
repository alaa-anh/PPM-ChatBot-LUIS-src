using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;

namespace Common
{
    public class Mongo
    {
        private IMongoDatabase _database;

        public Mongo()
        {
            string _connectionString = ConfigurationManager.AppSettings["MONGO_CONNECTIONSTRING"];
            MongoUrl mongoUrl = MongoUrl.Create(_connectionString);
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(mongoUrl.Server.Host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity("MongoDBChatBot", "mongodb-chatbot");
            MongoIdentityEvidence evidence = new PasswordEvidence("giXFEwkRQ1DWyNCFim7VnOGNb6KOkjYbdXDYIvbmtKwJrt0uEm573wzaccGFbw5WkVE9hqTEo7x2MTHNGPAkWA==");
            //settings.Credentials = new List<MongoCredential>(){
            //    new MongoCredential("SCRAM-SHA-1", identity, evidence)
            //};




            //MongoClient _client = new MongoClient(new MongoClientSettings
            //{
            //    Server = new MongoServerAddress(mongoUrl.Server.Host, mongoUrl.Server.Port)

            //});
            MongoClient _client = new MongoClient(settings);

            _database = _client.GetDatabase(MongoUrl.Create(_connectionString).DatabaseName, null);
        }

        public void Insert<T>(string collectionName, T document)
        {
            IMongoCollection<T> collection = _database.GetCollection<T>(collectionName);
            collection.InsertOne(document);
        }

        public T Get<T>(string collectionName, string property, string value)
        {
            IMongoCollection<T> collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq<string>(property, value);
            return collection.Find(filter).SingleOrDefault();
        }
    }
}