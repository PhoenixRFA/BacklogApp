using BacklogApp.IntegrationTests.Helpers;
using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Users;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class UsersTests : TestBase
    {
        public UsersTests() : base(injectTestAuthentication: true)
        {
            User = AddNewUser(TestsConstants.UserName, TestsConstants.UserEmail, TestsConstants.Password)!;
        }

        private readonly UserModel User;

        [Fact]
        public async Task CheckEmail()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.GetAsync("api/users/emailExists/" + TestsConstants.UserEmail);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            ResponseResult? result = await resp.Content.ReadFromJsonAsync<ResponseResult>();

            Assert.NotNull(result);
            Assert.True(result!.Result);
        }

        [Fact]
        public async Task ChangeEmail()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PutAsJsonAsync("api/users/email", new ChangeEmailModel { Email = TestsConstants.NewUserEmail });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            UserModel? updatedUser = GetRawUser(User.Id);

            Assert.NotNull(updatedUser);
            Assert.Equal(TestsConstants.NewUserEmail, updatedUser!.Email);
        }

        [Fact]
        public async Task ChangeName()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage resp = await client.PutAsJsonAsync("api/users/name", new ChangeNameModel { Name = TestsConstants.NewUsername });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            UserModel? updatedUser = GetRawUser(User.Id);

            Assert.NotNull(updatedUser);
            Assert.Equal(TestsConstants.NewUsername, updatedUser!.Name);
        }

        [Fact]
        public async Task RestorePassword()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("api/users/restorePassword", new ChangeEmailModel { Email = User.Email });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            UserModel updatedUser = GetRawUser(User.Id)!;

            Assert.NotEqual(User.PasswordHash, updatedUser.PasswordHash);
        }

        private class ResponseResult
        {
            public bool Result { get; set; }
        }

        internal class ChangePasswordResultModel
        {
            public BearerViewModel? Token { get; set; }
        }
    }
}
