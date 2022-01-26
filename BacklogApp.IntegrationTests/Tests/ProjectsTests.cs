using BacklogApp.IntegrationTests.Helpers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using MongoDB.Bson;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class ProjectsTests : TestBase
    {
        public ProjectsTests() : base(injectTestAuthentication: true)
        {
            User = AddNewUser(TestsConstants.UserName, TestsConstants.UserEmail, TestsConstants.Password)!;
            AltUser = AddNewUser(TestsConstants.UserName + " - Alt", "alt" + TestsConstants.UserEmail, TestsConstants.Password)!;

            Project1 = AddNewProject(TestsConstants.ProjectName, User.Id);
            Project2 = AddNewProject(TestsConstants.ProjectName + " 2", User.Id);
            Project3 = AddNewProject(TestsConstants.ProjectName + " 3", User.Id);
        }

        private readonly UserModel User;
        private readonly UserModel AltUser;
        private readonly ProjectViewModel Project1;
        private readonly ProjectViewModel Project2;
        private readonly ProjectViewModel Project3;

        [Fact]
        public async Task Get()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.GetAsync("/api/projects");

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            MultipleResultsModel<ProjectViewModel>? res = await resp.Content.ReadFromJsonAsync<MultipleResultsModel<ProjectViewModel>>();

            Assert.NotNull(res);
            Assert.Equal(3, res!.Items!.Length);
        }

        [Fact]
        public async Task GetProject()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.GetAsync("/api/projects/" + Project1.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            SingleResultsModel<ProjectViewModel>? res = await resp.Content.ReadFromJsonAsync<SingleResultsModel<ProjectViewModel>>();

            Assert.NotNull(res);
            Assert.Equal(Project1.Id, res!.Item!.Id);
            Assert.Equal(Project1.Name, res!.Item!.Name);
        }

        [Fact]
        public async Task Create()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PostAsJsonAsync("/api/projects", new CreateProjectModel { Name = TestsConstants.ProjectName + " - New Project" });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            Assert.NotNull(resp.Headers.Location);

            SingleResultsModel<ProjectViewModel>? res = await client.GetFromJsonAsync<SingleResultsModel<ProjectViewModel>>(resp.Headers.Location);

            Assert.NotNull(res);
            Assert.Equal(TestsConstants.ProjectName + " - New Project", res!.Item!.Name);
        }

        [Fact]
        public async Task ChangeName()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PutAsJsonAsync("/api/projects/" + Project1.Id + "/name", new ChangeProjectNameModel { Name = TestsConstants.ProjectName + " - New name" });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            SingleResultsModel<ProjectViewModel>? res = await client.GetFromJsonAsync<SingleResultsModel<ProjectViewModel>>("/api/projects/" + Project1.Id);

            Assert.Equal(TestsConstants.ProjectName + " - New name", res!.Item!.Name);
        }

        [Fact]
        public async Task AddUser()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PostAsync("/api/projects/" + Project1.Id + "/users/" + AltUser.Id, null);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            ProjectModel? proj = GetRawProject(Project1.Id);

            var user = proj.Users.FirstOrDefault(x=>x.Id == ObjectId.Parse(AltUser.Id));
            Assert.NotNull(user);
        }

        [Fact]
        public async Task RemoveUser()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.DeleteAsync("/api/projects/" + Project2.Id + "/users/" + User.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            ProjectModel? proj = GetRawProject(Project2.Id);

            var user = proj.Users.FirstOrDefault(x => x.Id == ObjectId.Parse(User.Id));
            Assert.Null(user);
        }

        [Fact]
        public async Task Delete()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.DeleteAsync("/api/projects/" + Project3.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            ProjectModel? proj = GetRawProject(Project3.Id);
            Assert.Null(proj);
        }
    }
}
