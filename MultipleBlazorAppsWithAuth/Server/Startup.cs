using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultipleBlazorAppsWithAuth.Server.Data;
using MultipleBlazorAppsWithAuth.Server.Models;
using System;
using System.Linq;

namespace MultipleBlazorAppsWithAuth.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.Use((context, next) =>
            {
                if (!context.Request.Path.HasValue)
                    return next();

                var pa = context.Request.Path.Value;
                Console.WriteLine(pa);

                if 
                (pa.Contains("favicon.ico")
          
            )
                    //I am totaly unaware why, but this is the only way that make it work..
             context.Request.Path = "/whatever" + context.Request.Path;

                return next();
            });


            app.MapWhen(ctx => !ctx.Request.Path.StartsWithSegments("/secondapp"), first =>
            {
                first.UseBlazorFrameworkFiles();
                first.UseStaticFiles();

                first.UseRouting();
                first.UseIdentityServer();
                first.UseAuthentication();
                first.UseAuthorization();
                first.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("{*path:}", "index.html");//only naked domain
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/secondapp"), first =>
            {
                first.UseBlazorFrameworkFiles("/secondapp");
                first.UseStaticFiles();
                first.UseStaticFiles("/secondapp");

                first.UseRouting();
                first.UseIdentityServer();
                first.UseAuthentication();
                first.UseAuthorization();
                first.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("secondapp/{*path:nonfile}", "secondapp/index.html");
                });
            });


        }
    }
}
