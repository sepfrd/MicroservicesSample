using System.Text;
using ChangeDataCaptureHub.Common.Dtos;
using ChangeDataCaptureHub.ExternalService.RabbitMQ.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChangeDataCaptureHub.ExternalService.RabbitMQ;

public sealed class MessageBusSubscriber : BackgroundService
{
    private readonly IConnection _rabbitMqConnection;
    private readonly IEventProcessor _eventProcessor;
    private readonly RabbitMqConfigurationDto _rabbitMqConfigurationDto;

    public MessageBusSubscriber(IConnection rabbitMqConnection, IConfiguration configuration, IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
        _rabbitMqConnection = rabbitMqConnection;
        _rabbitMqConfigurationDto = configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfigurationDto>()!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _rabbitMqConnection.CreateChannelAsync(cancellationToken: stoppingToken);

        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgument) =>
        {
            Console.WriteLine("Event received.");

            var body = eventArgument.Body;

            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            await _eventProcessor.ProcessEventAsync(notificationMessage, stoppingToken);
        };

        await channel.BasicConsumeAsync(
            _rabbitMqConfigurationDto.TriggerQueueName,
            true,
            consumer,
            cancellationToken: stoppingToken);
    }
}