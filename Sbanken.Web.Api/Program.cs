using Hub.Shared.Web.Api;
using Microsoft.Extensions.Hosting;
using Sbanken.Data;

namespace Sbanken.Web.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build()
            .Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return HostBuilder<DependencyRegistrationFactory, SbankenDbContext>.Create(args);
    }
}