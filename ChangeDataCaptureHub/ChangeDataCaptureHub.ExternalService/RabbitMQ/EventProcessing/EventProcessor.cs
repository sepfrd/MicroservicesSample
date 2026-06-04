using System.Text.Json;
using ChangeDataCaptureHub.Common.Dtos;
using ChangeDataCaptureHub.Common.Enums;
using ChangeDataCaptureHub.DataAccess;
using ChangeDataCaptureHub.Model.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeDataCaptureHub.ExternalService.RabbitMQ.EventProcessing;

public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ProcessEventAsync(string message, CancellationToken cancellationToken = default)
    {
        var eventDto = JsonSerializer.Deserialize<ToDoItemPublishedDto>(message);

        if (eventDto?.EventType is GrpcEventType.EntityCreated)
        {
            await AddToDoItemAsync(message, cancellationToken);
        }
    }

    private async Task AddToDoItemAsync(string toDoItemPublishedMessage, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<ToDoItemDocument>>();

        var toDoItemPublishedDto = JsonSerializer.Deserialize<ToDoItemPublishedDto>(toDoItemPublishedMessage);

        try
        {
            var toDoItemDocument = toDoItemPublishedDto?.ToDoItemDto.Adapt<ToDoItemDocument>();
            await repository.CreateOneAsync(toDoItemDocument!, cancellationToken);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Could not add toDoItem document to database due to an exception: {exception.Message}");
        }
    }
}