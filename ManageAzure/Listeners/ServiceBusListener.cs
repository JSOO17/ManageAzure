using Azure.Messaging.ServiceBus;
using ManageAzure.Config;
using ManageAzure.Interfaces;
using ManageAzure.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ManageAzure.Listeners
{
    public class ServiceBusListener : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger _logger;
        private readonly ServiceBusProcessor _processor;
        private readonly IAzureDevOpsRepository _azureServices;

        public ServiceBusListener(IOptions<ServiceBusOptions> options, IAzureDevOpsRepository azureServices, ILogger<ServiceBusListener> logger)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _processor = _client.CreateProcessor(options.Value.QueueName, new ServiceBusProcessorOptions());
            _logger = logger;
            _azureServices = azureServices;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var messageBody = args.Message.Body.ToString();
            _logger.LogInformation($"Received message: {messageBody}");

            var message = JsonSerializer.Deserialize<MessageModel>(messageBody) ?? throw new Exception(messageBody);

            if(message.Ticket != null)
            {
                await _azureServices.CreateIssue(message.Ticket);
            }
            else
            {
                await _azureServices.UploadFiles(message.File, message.WorkItemId);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Message handler encountered an exception");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.StopProcessingAsync(stoppingToken);
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _processor?.DisposeAsync();
            _client?.DisposeAsync();
            base.Dispose();
        }
    }
}
