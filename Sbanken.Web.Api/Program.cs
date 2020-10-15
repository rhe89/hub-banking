using Hub.Web.HostBuilder;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.Web.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string [] args)
        {
            return ApiHostBuilder.CreateHostBuilder<Startup, DependencyRegistrationFactory, SbankenDbContext>(args);
        }
    }
}
