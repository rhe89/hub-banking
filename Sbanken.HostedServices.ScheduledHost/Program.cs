using Hub.HostedServices.Schedule;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.HostedServices.ScheduledHost
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