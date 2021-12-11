using Hub.Shared.Web.BlazorServer;
using Microsoft.Extensions.Hosting;

namespace Sbanken.Web.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            HostBuilder<Startup, DependencyRegistrationFactory>.Create(args);
    }
}