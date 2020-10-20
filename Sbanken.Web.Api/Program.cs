using Hub.Web.HostBuilder;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new ApiHostBuilder<Startup, DependencyRegistrationFactory, SbankenDbContext>().CreateHostBuilder(args);
        }
    }
}
