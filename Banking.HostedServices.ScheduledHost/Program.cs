using Hub.Shared.HostedServices.Schedule;
using Microsoft.Extensions.Hosting;
using Banking.Data;

namespace Banking.HostedServices.ScheduledHost;

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