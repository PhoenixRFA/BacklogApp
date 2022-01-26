using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace BacklogApp.IntegrationTests.Helpers
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> opts, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(opts, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string userId = "1";
            if (Context.Request.Headers.ContainsKey(TestsConstants.UserIdHeaderName))
            {
                userId = Context.Request.Headers[TestsConstants.UserIdHeaderName];
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, TestsConstants.UserName)
            };

            ClaimsIdentity identity = new(claims, TestsConstants.TestAuthenticationScheme);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, TestsConstants.TestAuthenticationScheme);

            AuthenticateResult result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}
