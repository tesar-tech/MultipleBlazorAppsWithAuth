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


            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/FirstApp"), first =>
            {
                //first.UseBlazorFrameworkFiles();
                //first.UseHttpsRedirection();
                first.UseBlazorFrameworkFiles("/FirstApp");
                first.UseStaticFiles();
                first.UseStaticFiles("/FirstApp");

                first.UseRouting();
                first.UseIdentityServer();
                first.UseAuthentication();
                first.UseAuthorization();
                first.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("FirstApp/{*path:nonfile}", "FirstApp/index.html");
                });
            });

            app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/SecondApp"), first =>
            {
                //first.UseBlazorFrameworkFiles();
                //first.UseHttpsRedirection();
                first.UseBlazorFrameworkFiles("/SecondApp");
                first.UseStaticFiles();
                first.UseStaticFiles("/SecondApp");

                first.UseRouting();
                first.UseIdentityServer();
                first.UseAuthentication();
                first.UseAuthorization();
                first.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToFile("SecondApp/{*path:nonfile}", "SecondApp/index.html");
                });
            });




            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
