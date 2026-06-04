using ChangeDataCaptureHub.Business.Businesses;
using ChangeDataCaptureHub.Common.Dtos;
using ChangeDataCaptureHub.DataAccess;
using ChangeDataCaptureHub.DataAccess.Repositories;
using ChangeDataCaptureHub.ExternalService.RabbitMQ;
using ChangeDataCaptureHub.ExternalService.RabbitMQ.EventProcessing;
using ChangeDataCaptureHub.ExternalService.ToDoListManager;
using ChangeDataCaptureHub.ExternalService.ToDoListManager.ToDoListManagerGrpcService;
using ChangeDataCaptureHub.Model.Models;
using RabbitMQ.Client;

namespace ChangeDataCaptureHub.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection InjectControllers(this IServiceCollection services) => services.AddControllers().Services;

    public static IServiceCollection InjectDatabaseSettings(this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

    public static IServiceCollection InjectRepositories(this IServiceCollection services) =>
        services.AddScoped<IBaseRepository<ToDoItemDocument>, ToDoItemRepository>();

    public static IServiceCollection InjectBusinesses(this IServiceCollection services) =>
        services.AddScoped<ToDoItemBusiness>();

    internal static IServiceCollection InjectExternalServices(this IServiceCollection services) =>
        services
            .AddSingleton<IEventProcessor, EventProcessor>()
            .AddScoped<IToDoListManagerDataClient, ToDoListManagerDataClient>()
            .AddScoped<ToDoListManagerRestService>();

    internal static IServiceCollection InjectMessageBusSubscriber(this IServiceCollection services) =>
        services.AddHostedService<MessageBusSubscriber>();

    internal static async Task<IServiceCollection> InjectRabbitMqAsync(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfigurationDto = configuration.GetSection("RabbitMqConfiguration").Get<RabbitMqConfigurationDto>()!;

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfigurationDto.Host,
            Port = rabbitMqConfigurationDto.Port
        };

        var rabbitMqConnection = await factory.CreateConnectionAsync();

        await using var rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync();

        await rabbitMqChannel.ExchangeDeclareAsync(rabbitMqConfigurationDto.TriggerExchangeName, type: ExchangeType.Fanout, true);
        await rabbitMqChannel.QueueDeclareAsync(rabbitMqConfigurationDto.TriggerQueueName, true, false, false);

        await rabbitMqChannel.QueueBindAsync(
            rabbitMqConfigurationDto.TriggerQueueName,
            rabbitMqConfigurationDto.TriggerExchangeName,
            rabbitMqConfigurationDto.TriggerQueueName);

        Console.WriteLine("Listening on the Message Bus");

        rabbitMqConnection.ConnectionShutdownAsync += async (sender, shutdownEventArgs) =>
            await Console.Out.WriteLineAsync($"RabbitMQ connection was shutdown by {sender}, event args: {shutdownEventArgs}");

        services.AddSingleton(rabbitMqConnection);

        return services;
    }
}