using AutoMapper;
using Hub.HostedServices.Tasks;
using Hub.HostedServices.Timer;
using Hub.Storage.Repository.AutoMapper;
using Hub.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.BackgroundTasks;
using Sbanken.Core.Integration;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Integration;

namespace Sbanken.BackgroundWorker
{
    public class DependencyRegistrationFactory : DependencyRegistrationFactoryBase<SbankenDbContext>
    {
        protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
            serviceCollection.AddSingleton<IBackgroundTask, UpdateAccountsTask>();
            serviceCollection.AddSingleton<IBackgroundTask, UpdateTransactionsTask>();
            
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddHostedServiceProfiles();
                c.AddSbankenProfiles();
            });
        }
    }
}