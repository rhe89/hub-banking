using Hub.HostedServices.Commands.Logging.Core;
using Hub.HostedServices.ServiceBusQueue;
using Hub.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Sbanken.HostedServices.ServiceBusQueueHost.Commands;

namespace Sbanken.HostedServices.ServiceBusQueueHost.QueueListenerServices
{
    public class UpdateSbankenTransactionsQueueListener : ServiceBusHostedService
    {
        public UpdateSbankenTransactionsQueueListener(ILogger<UpdateSbankenTransactionsQueueListener> logger, 
            ICommandLogFactory commandLogFactory, 
            UpdateSbankenTransactionsCommand command, 
            IQueueProcessor queueProcessor) : base(logger, 
                                                    commandLogFactory, 
                                                    command, 
                                                    queueProcessor)
        {
        }
    }
}