using BacklogApp.IntegrationTests.Helpers;
using BacklogApp.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class ResourcesTests : TestBase
    {
        public ResourcesTests() : base(injectTestAuthentication: true)
        {
            User = AddNewUser(TestsConstants.UserName, TestsConstants.UserEmail, TestsConstants.Password)!;
            
            byte[] data = GenerateData(TestsConstants.FileSize);
            GetFileId = AddNewFile(TestsConstants.FileReadName, TestsConstants.FileCode, TestsConstants.FileMime, User.Id, data);
            DeleteFileId = AddNewFile(TestsConstants.FileDeleteName, TestsConstants.FileCode, TestsConstants.FileMime, User.Id, data);
        }

        private readonly UserModel User;
        private readonly string GetFileId;
        private readonly string DeleteFileId;

        [Fact]
        public async Task GetFile()
        {
            HttpClient client = CreateClient();

            HttpResponseMessage resp = await client.GetAsync("api/resources/" + GetFileId);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            Assert.Equal(TestsConstants.FileSize, resp.Content.Headers.ContentLength);
            Assert.Equal(TestsConstants.FileMime, resp.Content.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task UploadFile()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            var bytes = GenerateData(TestsConstants.FileSize);

            var bc = new ByteArrayContent(bytes);
            bc.Headers.ContentType = new MediaTypeHeaderValue(TestsConstants.FileMime);

            var content = new MultipartFormDataContent
            {
                { bc, "file", TestsConstants.FileCreateName }
            };
            HttpResponseMessage? resp = await client.PostAsync("api/resources/" + TestsConstants.FileCode, content);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            ResponseResult? res = await resp.Content.ReadFromJsonAsync<ResponseResult>();

            Assert.NotNull(res);
            Assert.NotNull(res!.ResourceId);

            resp = await client.GetAsync("api/resources/" + res.ResourceId);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            Assert.Equal(TestsConstants.FileCreateName, resp.Content.Headers.ContentDisposition!.FileName);
            Assert.Equal(TestsConstants.FileMime, resp.Content.Headers.ContentType!.MediaType);

            byte[] hash = MD5.HashData(bytes);
            
            byte[] responseFile = await resp.Content.ReadAsByteArrayAsync();
            byte[] contentHash = MD5.HashData(responseFile);

            Assert.Equal(hash, contentHash);
        }

        [Fact]
        public async Task DeleteFile()
        {
            HttpClient client = CreateClient();
            SetUserIdHeader(client, User.Id);

            HttpResponseMessage? resp = await client.GetAsync("api/resources/" + DeleteFileId);
            
            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            resp = await client.DeleteAsync("api/resources/" + DeleteFileId);

            resp.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            resp = await client.GetAsync("api/resources/" + DeleteFileId);
            
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        internal class ResponseResult
        {
            public string? ResourceId { get; set; }
        }
    }
}
