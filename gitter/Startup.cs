using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gitter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var plantumlJar = @"C:\bin\plantuml.jar";
            var processRunner = new RealProcessRunner();
            var plantumlRenderer = new PlantumlRenderer(processRunner, plantumlJar);
            var markdownRenderer = new MarkdownRenderer(plantumlRenderer);

            var gitRepository = @"C:\work\chp";
            services.AddSingleton<IMarkdownRenderer>(markdownRenderer);
            services.AddSingleton<IContentProvider>(_ => (IContentProvider) new FileSystemContentProvider(gitRepository));
            services.AddSingleton<IContentGrep>(_ => new GitContentGrep(processRunner, gitRepository));
            services.AddSingleton<IPlantumlRenderer>(_ => plantumlRenderer);
            services.AddSingleton<IProcessRunner>(processRunner);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "grep",
                    template: "grep", defaults: new { controller = "Content", action = "Grep" });

                routes.MapRoute(
                    name: "uml",
                    template: "plantuml/{*filename}", defaults: new { controller = "Plantuml", action = "Index" });

                routes.MapRoute(
                    name: "raw",
                    template: "raw/{*path}", defaults: new { controller = "Content", action = "Raw" });

                routes.MapRoute(
                    name: "content",
                    template: "{*path}", defaults: new { controller = "Content", action = "Index"});
            });
        }
    }
}
