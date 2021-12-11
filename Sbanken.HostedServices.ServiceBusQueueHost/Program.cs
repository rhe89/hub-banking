using Hub.Shared.HostedServices.ServiceBusQueue;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.HostedServices.ServiceBusQueueHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new Bootstrapper<DependencyRegistrationFactory, SbankenDbContext>(args, "SQL_DB_SBANKEN")
                .CreateHostBuilder()
                .Build()
                .Run();
        }
    }
}