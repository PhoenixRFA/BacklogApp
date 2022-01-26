using BacklogApp.Models;
using BacklogApp.Models.Db;
using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Diagnostics;

namespace BacklogApp.IntegrationTests.Helpers
{
    public class DbFixture : IDisposable
    {
        public MongoClient Client { get; }
        public IMongoDatabase Database { get; }
        public string ConnectionString { get; }

        public IMongoCollection<UserModel> UsersCollection { get; }
        public IMongoCollection<ProjectModel> ProjectsCollection { get; }
        public IMongoCollection<TaskModel> TasksCollection { get; }

        private readonly MongoDbRunner _mongoRunner;
        private readonly string _databaseName = "test-database";

        public DbFixture()
        {
            // initializes the instance
            _mongoRunner = MongoDbRunner.Start();

            // store the connection string with the chosen port number
            ConnectionString = _mongoRunner.ConnectionString;
            Debug.WriteLine($"MongoDb: {ConnectionString}");
            // create a client and database for use outside the class
            Client = new MongoClient(ConnectionString);

            Database = Client.GetDatabase(_databaseName);

            UsersCollection = Database.GetCollection<UserModel>(DbCollections.Users);
            ProjectsCollection = Database.GetCollection<ProjectModel>(DbCollections.Projects);
            TasksCollection = Database.GetCollection<TaskModel>(DbCollections.Tasks);
        }

        public void Dispose()
        {
            _mongoRunner.Dispose();
            Debug.WriteLine($"||||||| Dispose Called |||||||");
        }
    }
}
