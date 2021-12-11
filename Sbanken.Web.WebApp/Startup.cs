using AutoMapper;
using Hub.Shared.Storage.Repository;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Providers;
using Sbanken.Services;

namespace Sbanken.Web.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddRazorPages();
            serviceCollection.AddServerSideBlazor();
            serviceCollection.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = _configuration.GetValue<string>("AI_CONNECTION_STRING")
            });
            
            serviceCollection.AddDatabase<SbankenDbContext>(_configuration, "SQL_DB_SBANKEN", "Sbanken.Data");
            serviceCollection.AddTransient<ITransactionService, TransactionService>();
            serviceCollection.AddTransient<ITransactionProvider, TransactionProvider>();
            
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddSbankenProfiles();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}