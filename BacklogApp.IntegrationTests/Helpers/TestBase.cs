using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using BacklogApp.Models.Tasks;
using BacklogApp.Repository;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BacklogApp.IntegrationTests.Helpers
{
    public class TestBase : IDisposable
    {
        private readonly BacklogApplication _app;
        protected IServiceProvider Services { get; }

        public TestBase(bool injectTestAuthentication = false)
        {
            _app = new BacklogApplication(injectTestAuthentication);
            Services = _app.Services;
        }

        public virtual void Dispose()
        {
            _app.Dispose();
        }

        //protected UserModel? GetUserByEmail(string email) => _app.MongoRunner.UsersCollection.Find(x => x.Email == email).FirstOrDefault();

        protected HttpClient CreateClient() => _app.CreateClient();

        protected UserModel? GetRawUser(string id) => _app.MongoRunner.UsersCollection.Find(x => x.Id == id).FirstOrDefault();

        protected ProjectViewModel AddNewProject(string name, string userId)
        {
            ProjectsManager projMng = (ProjectsManager)Services.GetService(typeof(ProjectsManager))!;

            return projMng.Create(name, userId);
        }

        protected ProjectModel GetRawProject(string id) => _app.MongoRunner.ProjectsCollection.Find(x => x.Id == id).FirstOrDefault();

        protected UserModel? AddNewUser(string name, string email, string password)
        {
            UsersManager userMng = (UsersManager)Services.GetService(typeof(UsersManager))!;

            return userMng.Add(name, email, password);
        }

        protected TaskModel GetRawTask(string id) => _app.MongoRunner.TasksCollection.Find(x => x.Id == id).FirstOrDefault();

        protected TaskViewModel? AddNewTask(string name, string projectId, string userId, string description, DateTime deadline, string priority, string assessment)
        {
            TasksManager taskMng = (TasksManager)Services.GetService(typeof(TasksManager))!;

            return taskMng.Create(name, projectId, userId, description, deadline, priority, assessment);
        }

        protected static void SetUserIdHeader(HttpClient client, string userId) => client.DefaultRequestHeaders.TryAddWithoutValidation(TestsConstants.UserIdHeaderName, userId);

        protected static IList<SetCookieHeaderValue> GetCookies(HttpResponseMessage response) =>
            SetCookieHeaderValue.ParseList(
                response.Headers.GetValues(HeaderNames.SetCookie).ToList()
            );

        protected string AddNewFile(string name, string code, string mime, string userId, byte[] data)
        {
            IResourceRepository repo = (IResourceRepository)Services.GetService(typeof(IResourceRepository))!;
            IGridFSBucket gridFs = (IGridFSBucket)Services.GetService(typeof(IGridFSBucket))!;

            ObjectId id = gridFs.UploadFromBytes(name, data);
            var res = new ResourceModel
            {
                FileId = id,
                MimeType = mime,
                OriginalName = name,
                Owner = ObjectId.Parse(userId),
                Code = code
            };
            repo.Create(res);

            return res.Id;
        }

        protected byte[] GenerateData(int size)
        {
            byte[] data = new byte[size];
            Random rnd = new();
            
            rnd.NextBytes(data);

            return data;
        }
    }
}
