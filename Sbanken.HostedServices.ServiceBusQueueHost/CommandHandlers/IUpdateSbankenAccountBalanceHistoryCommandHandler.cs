using System.Threading.Tasks;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers
{
    public interface IUpdateSbankenAccountBalanceHistoryCommandHandler
    {
        Task UpdateAccountBalance();
    }
}