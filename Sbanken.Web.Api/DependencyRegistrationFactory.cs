using AutoMapper;
using Hub.HostedServices.Tasks;
using Hub.Storage.Repository.AutoMapper;
using Hub.Web.Api;
using Hub.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.BackgroundTasks;
using Sbanken.Core.Integration;
using Sbanken.Core.Providers;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Integration;
using Sbanken.Providers;

namespace Sbanken.Web.Api
{
    public class DependencyRegistrationFactory : DependencyRegistrationFactoryWithHostedServiceBase<SbankenDbContext>
    {
        public DependencyRegistrationFactory() : base("SQL_DB_SBANKEN", "Sbanken.Data")
        {
        }

        protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<IAccountProvider, AccountProvider>();
            serviceCollection.AddTransient<ITransactionProvider, TransactionProvider>();
            serviceCollection.AddTransient<ITransactionSummaryProvider, TransactionSummaryProvider>();
            serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
            serviceCollection.AddScoped<UpdateAccountsTask>();
            serviceCollection.AddScoped<UpdateTransactionsTask>();
            serviceCollection.AddScoped<WorkerLogMaintenanceBackgroundTask>();
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddHostedServiceProfiles();
                c.AddSbankenProfiles();
            });
        }
    }
}