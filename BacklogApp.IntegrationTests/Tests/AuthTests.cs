using BacklogApp.IntegrationTests.Helpers;
using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Users;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class AuthTests : TestBase
    {
        public AuthTests() : base()
        {
            AddNewUser(TestsConstants.UserName, TestsConstants.UserEmail, TestsConstants.Password);
        }

        [Fact]
        public async Task Register()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage res = await client.PostAsJsonAsync("api/auth/register", new RegisterModel { Email = "test@email.com", Name = "testUser" });

            res.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        }

        [Fact]
        public async Task Login()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("api/auth/login", new LoginModel { Username = TestsConstants.UserEmail, Password = TestsConstants.Password });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            LoginResultModel? result = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            Assert.NotNull(result);

            Assert.Equal(TestsConstants.UserEmail, result!.User!.Email);
            Assert.Equal(TestsConstants.UserName, result.User.Username);

            IList<SetCookieHeaderValue> cookies = GetCookies(resp);
            SetCookieHeaderValue? refreshTokenCookie = cookies.FirstOrDefault(x => x.Name == TestsConstants.RefreshTokenCookieName);

            Assert.NotNull(refreshTokenCookie);
            Assert.True(refreshTokenCookie!.HttpOnly);
        }

        [Fact]
        public async Task Logout()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("api/auth/login", new LoginModel { Username = TestsConstants.UserEmail, Password = TestsConstants.Password });
            resp.EnsureSuccessStatusCode();

            LoginResultModel? result = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token!.Bearer);

            resp = await client.PostAsync("api/auth/logout", null);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            IList<SetCookieHeaderValue> cookies = GetCookies(resp);
            SetCookieHeaderValue? refreshTokenCookie = cookies.FirstOrDefault(x => x.Name == TestsConstants.RefreshTokenCookieName);

            Assert.NotNull(refreshTokenCookie);
            Assert.True(refreshTokenCookie!.Expires < DateTime.Today);
        }

        [Fact]
        public async Task GetMe()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("/api/auth/login", new LoginModel { Username = TestsConstants.UserEmail, Password = TestsConstants.Password });
            resp.EnsureSuccessStatusCode();

            LoginResultModel? result = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token!.Bearer);

            resp = await client.GetAsync("api/auth/me");

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            UserViewModel? user = await resp.Content.ReadFromJsonAsync<UserViewModel>();

            Assert.NotNull(user);
            Assert.Equal(TestsConstants.UserName, user!.Username);
            Assert.Equal(TestsConstants.UserEmail, user.Email);
        }

        [Fact]
        public async Task RefreshToken()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("/api/auth/login", new LoginModel { Username = TestsConstants.UserEmail, Password = TestsConstants.Password });
            resp.EnsureSuccessStatusCode();

            LoginResultModel? result = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token!.Bearer);

            resp = await client.PostAsync("api/auth/refresh-token", null);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            LoginResultModel? loginResult = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            Assert.NotNull(loginResult);
            Assert.Equal(TestsConstants.UserName, loginResult!.User!.Username);
            Assert.Equal(TestsConstants.UserEmail, loginResult!.User!.Email);

            IList<SetCookieHeaderValue> cookies = GetCookies(resp);
            SetCookieHeaderValue? refreshTokenCookie = cookies.FirstOrDefault(x => x.Name == TestsConstants.RefreshTokenCookieName);

            Assert.NotNull(refreshTokenCookie);
            Assert.True(refreshTokenCookie!.HttpOnly);
        }

        [Fact]
        public async Task ChangePassword()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.PostAsJsonAsync("/api/auth/login", new LoginModel { Username = TestsConstants.UserEmail, Password = TestsConstants.Password });
            resp.EnsureSuccessStatusCode();

            LoginResultModel? result = await resp.Content.ReadFromJsonAsync<LoginResultModel>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token!.Bearer);

            UserModel? notUpdatedUser = GetRawUser(result!.User!.Id);

            resp = await client.PutAsJsonAsync($"api/auth/password", new ChangePasswordModel { OldPassword = TestsConstants.Password, NewPassword = TestsConstants.NewPassword });

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            UserModel? updatedUser = GetRawUser(result!.User!.Id);

            Assert.NotNull(updatedUser);
            Assert.NotEqual(notUpdatedUser!.PasswordHash, updatedUser!.PasswordHash);
        }

        internal class LoginResultModel
        {
            public BearerViewModel? Token { get; set; }
            public UserViewModel? User { get; set; }
        }
    }
}
