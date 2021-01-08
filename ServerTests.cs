using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspIntegrationTestMemoryLeakIssue
{
    [TestClass]
    public class ServerTests
    {
        [DataTestMethod]
        [DataRow, DataRow, DataRow]
        public async Task RealServerTest()
        {
            using var webHost = WebHost.CreateDefaultBuilder()
                .UseUrls("http://localhost/")
                .UseStartup<Startup1>()
                .Build();

            await webHost.StartAsync();

            using HttpClient client = new();

            Assert.AreEqual("Hello World!", (await (await client.GetAsync("http://localhost/")).EnsureSuccessStatusCode().Content.ReadAsStringAsync()));
        }

        [DataTestMethod]
        [DataRow, DataRow, DataRow]
        public async Task TestServerTest_WebHost()
        {
            using TestServer server = new(WebHost.CreateDefaultBuilder().UseUrls("http://localhost/").UseStartup<Startup1>());

            Assert.AreEqual("Hello World!", (await (await server.CreateClient().GetAsync("/")).EnsureSuccessStatusCode().Content.ReadAsStringAsync()));
        }

        [DataTestMethod]
        [DataRow, DataRow, DataRow]
        public async Task TestServerTest_GenericHost()
        {
            using IHost host = new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder
                      .UseTestServer()
                      .UseUrls("http://localhost/")
                      .UseStartup<Startup2>();
                })
                .Build();

            await host.StartAsync();

            using TestServer server = host.GetTestServer();

            Assert.AreEqual("Hello World!", (await (await server.CreateClient().GetAsync("/")).EnsureSuccessStatusCode().Content.ReadAsStringAsync()));
        }
    }

    public class Startup1
    {
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.Populate(services);

            IContainer container = builder.Build();

            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }

    public class Startup2
    {
        public void ConfigureContainer(ContainerBuilder builder)
        {
            
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
