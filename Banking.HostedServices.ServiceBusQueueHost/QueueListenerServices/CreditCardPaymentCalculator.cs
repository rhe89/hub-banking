using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class CreditCardPaymentCalculator : ServiceBusHostedService
{
    public CreditCardPaymentCalculator(ILogger<CreditCardPaymentCalculator> logger, 
                                      IConfiguration configuration,
                                      CreditCardPaymentCalculatorCommand command, 
                                      IQueueProcessor queueProcessor,
                                      TelemetryClient telemetryClient) : base(logger, 
                                                                              configuration,
                                                                              command, 
                                                                              queueProcessor,
                                                                              telemetryClient)
    {
    }
}