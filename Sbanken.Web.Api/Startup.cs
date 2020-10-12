using Hub.Web.Startup;
using Microsoft.Extensions.Configuration;
using Sbanken.Data;

namespace Sbanken.Web.Api
{
    public class Startup : ApiStartup<SbankenDbContext, DependencyRegistrationFactory>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
            
        }
    }
}
