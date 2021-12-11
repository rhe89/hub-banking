using AutoMapper;
using Hub.Shared.Storage.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Providers;
using Sbanken.Services;

namespace Sbanken.Web.WebApp
{
    public class DependencyRegistrationFactory : Hub.Shared.Web.BlazorServer.DependencyRegistrationFactory
    {
        protected override void AddBlazorExtras(IServiceCollection serviceCollection, IConfiguration configuration)
        {
        }

        protected override void AddHttpClients(IServiceCollection serviceCollection, IConfiguration configuration)
        {
        }

        protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDatabase<SbankenDbContext>(configuration, "SQL_DB_SBANKEN", "Sbanken.Data");
            serviceCollection.AddTransient<ITransactionService, TransactionService>();
            serviceCollection.AddTransient<ITransactionProvider, TransactionProvider>();
            
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddSbankenProfiles();
            });
        }
    }
}