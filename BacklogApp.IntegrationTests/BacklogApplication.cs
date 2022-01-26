using BacklogApp.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Net.Http;
using System.Text.Encodings.Web;

namespace BacklogApp.IntegrationTests
{
    internal class BacklogApplication : WebApplicationFactory<Program>
    {
        internal DbFixture MongoRunner { get; }
        private readonly bool _injectTestAuthentication;

        internal BacklogApplication(bool injectTestAuthentication = false) : base()
        {
            _injectTestAuthentication = injectTestAuthentication;

            MongoRunner = new DbFixture();
        }

        //protected override IHost CreateHost(IHostBuilder builder)
        //{
        //    builder.ConfigureServices(services =>
        //    {
        //        services.RemoveAll<IMongoDatabase>();
        //        services.TryAddTransient<IMongoDatabase>(_ => MongoRunner.Database);

        //        if (_injectTestAuthentication)
        //        {
        //            services.AddAuthentication(TestsConstants.TestAuthenticationScheme)
        //                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestsConstants.TestAuthenticationScheme, _ => { });
        //        }
        //    });

        //    return base.CreateHost(builder);
        //}

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMongoDatabase>();
                services.TryAddTransient<IMongoDatabase>(_ => MongoRunner.Database);

                if (_injectTestAuthentication)
                {
                    services.AddAuthentication(TestsConstants.TestAuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestsConstants.TestAuthenticationScheme, opts => { });
                }
            });

            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MongoRunner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
