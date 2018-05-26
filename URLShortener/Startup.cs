using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using URLShortener.Data;
using URLShortener.Models;

namespace URLShortener
{
    public class Startup
    {
        IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json").Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UrlShortenerDB")));


            // configuring password policy for users
            services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;

                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            #region services for TempData (as for now)
            services.AddMemoryCache();
            services.AddSession();
            #endregion
            
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();
            app.UseStatusCodePages();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add support for node_modules but only during development **temporary**
            // using npm instead of bower
            // (solution from some blog) - not rly important right now
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                RequestPath = new PathString("/vendor")
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "",
                    template: "{action=Index}",
                    defaults: new { controller = "Home" });

                routes.MapRoute(
                    name: "targetRedirection",
                    template: "{urlName}",
                    defaults: new { controller = "Home", action = "RedirectToTarget" });

            });
        }

        
    }
}
