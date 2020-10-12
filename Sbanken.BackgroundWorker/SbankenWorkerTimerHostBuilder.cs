using Hub.HostedServices.Tasks;
using Hub.HostedServices.Timer;
using Hub.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.BackgroundTasks;
using Sbanken.Data;
using Sbanken.Integration;

namespace Sbanken.BackgroundWorker
{
    public class SbankenWorkerTimerHostBuilder : TimerHostBuilder<SbankenDbContext>
    {
        internal SbankenWorkerTimerHostBuilder(string[] args) : base(args, "SQL_DB_SBANKEN")
        {
        }

        protected override void RegisterDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
            serviceCollection.AddSingleton<IBackgroundTask, UpdateAccountsTask>();
            serviceCollection.AddSingleton<IBackgroundTask, UpdateTransactionsTask>();
        }
    }
}