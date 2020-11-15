using Hub.HostedServices.Timer;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.BackgroundWorker
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new BackgroundWorker<DependencyRegistrationFactory, SbankenDbContext>(args, "SQL_DB_SBANKEN")
                .CreateHostBuilder()
                .Build()
                .Run();
        }
    }
}