using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IHostingEnvironment environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            this.environment = environment;
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

            var plantumlJar = Configuration["PlantUmlJar"] ?? 
                Path.Combine(environment.ContentRootPath, "plantuml.jar");

            var processRunner = new RealProcessRunner(
                new[] { @"java\bin", @"graphviz\release\bin", @"Git\Cmd" }.Select(_ => Utils.LookUpwardsForSubdirectory(_))
                .Where(_ => _.HasValue).Select(_ => _.Value),
                new Dictionary<string, string>
                {
                    { "GRAPHVIZ_DOT", Utils.LookUpwardsForSubdirectory(@"graphviz\release\bin").Select(_ => Path.Combine(_, "dot.exe")).ValueOr(String.Empty) }
                }
                );


            var plantumlRenderer = new PlantumlRenderer(processRunner, plantumlJar, Path.Combine(environment.ContentRootPath, "plantuml-1.0.0"));
            var markdownRenderer = new MarkdownRenderer(plantumlRenderer);

            var gitRepository = Configuration["Repository"];
            if (!Path.IsPathRooted(gitRepository))
            {
                gitRepository = Path.Combine(environment.ContentRootPath, gitRepository);
            }

            Utils.EnsureDirectoryExists(gitRepository);
            var git = (IGit) new Git(processRunner, gitRepository);
            services.AddSingleton(git);
            services.AddSingleton<IMarkdownRenderer>(markdownRenderer);
            services.AddSingleton<IContentProvider>(_ => (IContentProvider) new FileSystemContentProvider(gitRepository, git));
            services.AddSingleton<IContentGrep>(_ => new GitContentGrep(git));
            services.AddSingleton<IHistory>(_ => new GitLog(git));
            services.AddSingleton<IPlantumlRenderer>(_ => plantumlRenderer);
            services.AddSingleton<IProcessRunner>(processRunner);
            services.AddSingleton<LibGit2Sharp.IRepository>(new LibGit2Sharp.Repository(gitRepository));
        }

        IConfiguration configuration;

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

            app.UseWebSockets();
            app.UseHttpsRedirection();
            app.UsePathBase(Configuration["PathBase"]);
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
