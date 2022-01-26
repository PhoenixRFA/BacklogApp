using BacklogApp.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.IntegrationTests.Tests
{
    public class BasicTests : TestBase
    {
        [Theory]
        [InlineData("/online")]
        public async Task Test(string url)
        {
            HttpClient client = CreateClient();

            HttpResponseMessage res = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
    }
}
