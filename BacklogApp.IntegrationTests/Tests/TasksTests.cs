using BacklogApp.IntegrationTests.Helpers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using BacklogApp.Models.Tasks;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class TasksTests : TestBase
    {
        public TasksTests() : base(injectTestAuthentication: true)
        {
            User = AddNewUser(TestsConstants.UserName, TestsConstants.UserEmail, TestsConstants.Password)!;

            Project = AddNewProject(TestsConstants.ProjectName, User.Id)!;

            Task1 = AddNewTask(TestsConstants.TaskName + " 1", Project.Id, User.Id, TestsConstants.TaskDescription, DateTime.Parse(TestsConstants.TaskDeadline), TestsConstants.TaskPriorityCode, TestsConstants.TaskAssessment)!;
            Task2 = AddNewTask(TestsConstants.TaskName + " 2", Project.Id, User.Id, TestsConstants.TaskDescription, DateTime.Parse(TestsConstants.TaskDeadline), TestsConstants.TaskPriorityCode, TestsConstants.TaskAssessment)!;
            Task3 = AddNewTask(TestsConstants.TaskName + " 3", Project.Id, User.Id, TestsConstants.TaskDescription, DateTime.Parse(TestsConstants.TaskDeadline), TestsConstants.TaskPriorityCode, TestsConstants.TaskAssessment)!;
        }

        private readonly UserModel User;
        private readonly ProjectViewModel Project;
        private readonly TaskViewModel Task1;
        private readonly TaskViewModel Task2;
        private readonly TaskViewModel Task3;

        [Fact]
        public async Task GetInProject()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.GetAsync("api/tasks/project/" + Project.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var res = await resp.Content.ReadFromJsonAsync<MultipleResultsModel<TaskViewModel>>();

            Assert.NotNull(res);
            Assert.Equal(3, res!.Items!.Length);
        }

        [Fact]
        public async Task Get()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.GetAsync("api/tasks/" + Task1.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            SingleResultsModel<TaskViewModel>? res = await resp.Content.ReadFromJsonAsync<SingleResultsModel<TaskViewModel>>();

            Assert.NotNull(res);
            Assert.Equal(Task1.Id, res!.Item!.Id);
            Assert.Equal(Task1.Name, res!.Item!.Name);
            Assert.Equal(Task1.Description, res!.Item!.Description);
            Assert.Equal(Task1.Status, res!.Item!.Status);
            Assert.Equal(Task1.Deadline!.Value.ToUniversalTime(), res!.Item!.Deadline);
            Assert.Equal(Task1.Priority, res!.Item!.Priority);
            Assert.Equal(Task1.Assessment, res!.Item!.Assessment);
        }

        [Fact]
        public async Task Create()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            var deadline = new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            HttpResponseMessage resp = await client.PostAsJsonAsync("api/tasks", new CreateTaskModel
            {
                Name = TestsConstants.TaskName + " - New",
                Assessment = TestsConstants.TaskAssessment,
                Deadline = deadline,
                Description = TestsConstants.TaskDescription,
                Priority = TestsConstants.TaskPriorityCode,
                ProjectId = Project.Id
            });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            Assert.NotNull(resp.Headers.Location);

            SingleResultsModel<TaskViewModel>? result = await client.GetFromJsonAsync<SingleResultsModel<TaskViewModel>>(resp.Headers.Location);

            Assert.NotNull(result);
            Assert.Equal(TestsConstants.TaskName + " - New", result!.Item!.Name);
            Assert.Equal(TestsConstants.TaskAssessment, result!.Item!.Assessment);
            Assert.Equal(deadline, result!.Item!.Deadline);
            Assert.Equal(TestsConstants.TaskPriorityCode, result!.Item!.Priority);
            Assert.Equal(TestsConstants.TaskStatusDiscussion, result!.Item!.Status);
        }

        [Fact]
        public async Task Edit()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            var deadline = new DateTime(2020, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            HttpResponseMessage resp = await client.PutAsJsonAsync("api/tasks/" + Task2.Id, new EditTaskModel
            {
                Name = TestsConstants.TaskName + " - New name",
                Assessment = "5h",
                Deadline = deadline,
                Description = "No Description",
                Priority = "low"
            });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            TaskModel? task = GetRawTask(Task2.Id);

            Assert.NotNull(task);
            Assert.Equal(TestsConstants.TaskName + " - New name", task.Name);
            Assert.Equal("5h", task.Assessment);
            Assert.Equal(deadline, task.Deadline);
            Assert.Equal("No Description", task.Description);
            Assert.Equal(2, task.Priority);
        }

        [Theory]
        [InlineData(TestsConstants.TaskStatusDiscussion, TestsConstants.TaskStatusDiscussionShort)]
        [InlineData(TestsConstants.TaskStatusInWork, TestsConstants.TaskStatusInWorkShort)]
        [InlineData(TestsConstants.TaskStatusCompleted, TestsConstants.TaskStatusCompletedShort)]
        public async Task EditStatus(string status, string statusShort)
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PutAsync("api/tasks/" + Task2.Id + "/status/" + status, null);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            TaskModel? task = GetRawTask(Task2.Id);

            Assert.NotNull(task);
            Assert.Equal(statusShort, task.Status);
        }

        [Fact]
        public async Task Delete()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.DeleteAsync("api/tasks/" + Task3.Id);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            TaskModel? task = GetRawTask(Task3.Id);

            Assert.Null(task);
        }
    }
}
