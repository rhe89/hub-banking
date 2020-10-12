using Hub.HostedServices.Tasks;
using Hub.Web.DependencyRegistration;
using Hub.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.BackgroundTasks;
using Sbanken.Data;
using Sbanken.Integration;
using Sbanken.Providers;

namespace Sbanken.Web.Api
{
    public class DependencyRegistrationFactory : ApiWithQueueHostedServiceDependencyRegistrationFactory<SbankenDbContext>
    {
        public DependencyRegistrationFactory() : base("SQL_DB_SBANKEN", "Sbanken.Data")
        {
        }

        protected override void RegisterDomainDependenciesForQueueHostedService(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            
        }

        protected override void RegisterDomainDependenciesForApi(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<IAccountProvider, AccountProvider>();
            serviceCollection.AddTransient<ITransactionProvider, TransactionProvider>();
            serviceCollection.AddTransient<ITransactionSummaryProvider, TransactionSummaryProvider>();
        }

        protected override void RegisterSharedDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
            serviceCollection.AddScoped<UpdateAccountsTask>();
            serviceCollection.AddScoped<UpdateTransactionsTask>();
            serviceCollection.AddScoped<WorkerLogMaintenanceBackgroundTask>();
        }
    }
}