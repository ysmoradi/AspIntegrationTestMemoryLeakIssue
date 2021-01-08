using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspIntegrationTestMemoryLeakIssue
{
    [TestClass]
    public class ServerTests
    {
        [DataTestMethod]
        [DataRow, DataRow, DataRow]
        public async Task TestServerTest_WebHost()
        {
            using TestServer server = new(WebHost.CreateDefaultBuilder().UseUrls("http://localhost/").UseStartup<Startup1>());

            Assert.AreEqual("Hello World!", (await (await server.CreateClient().GetAsync("/")).EnsureSuccessStatusCode().Content.ReadAsStringAsync()));

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            Assert.AreEqual(1, GCContainer.References.Count(r => r.IsAlive));
        }
    }

    public static class GCContainer
    {
        public static List<WeakReference> References { get; set; } = new List<WeakReference>();
    }

    public class Startup1 // web host builder
    {
        public Startup1()
        {
            GCContainer.References.Add(new WeakReference(this));
        }

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return WindsorRegistrationHelper.CreateServiceProvider(new WindsorContainer(), services);
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
