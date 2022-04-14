using Hub.Shared.HostedServices.ServiceBusQueue;
using Microsoft.Extensions.Hosting;
using Banking.Data;

namespace Banking.HostedServices.ServiceBusQueueHost;

public static class Program
{
    public static void Main(string[] args)
    {
        new Bootstrapper<DependencyRegistrationFactory, BankingDbContext>(args, "SQL_DB_BANKING")
            .CreateHostBuilder()
            .Build()
            .Run();
    }
}