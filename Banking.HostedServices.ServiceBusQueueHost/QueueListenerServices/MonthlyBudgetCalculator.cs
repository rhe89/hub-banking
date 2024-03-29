using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class MonthlyBudgetCalculator : ServiceBusHostedService
{
    public MonthlyBudgetCalculator(ILogger<MonthlyBudgetCalculator> logger, 
                                       IConfiguration configuration,
                                       MonthlyBudgetCalculatorCommand command, 
                                       IQueueProcessor queueProcessor,
                                       TelemetryClient telemetryClient) : base(logger, 
                                                                               configuration,
                                                                               command, 
                                                                               queueProcessor,
                                                                               telemetryClient)
    {
    }
}