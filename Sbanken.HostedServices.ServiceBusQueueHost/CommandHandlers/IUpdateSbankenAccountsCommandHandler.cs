using System.Threading.Tasks;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers
{
    public interface IUpdateSbankenAccountsCommandHandler
    {
        Task UpdateAccounts();
    }
}