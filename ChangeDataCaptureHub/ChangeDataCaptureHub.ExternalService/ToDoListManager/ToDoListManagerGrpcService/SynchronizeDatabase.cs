using ChangeDataCaptureHub.DataAccess;
using ChangeDataCaptureHub.Model.Models;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeDataCaptureHub.ExternalService.ToDoListManager.ToDoListManagerGrpcService;

public static class SynchronizeDatabase
{
    public static async Task FetchNewEntitiesAsync(IServiceScopeFactory serviceScopeFactory)
    {
        using var serviceScope = serviceScopeFactory.CreateScope();

        var grpcClient = serviceScope.ServiceProvider.GetRequiredService<IToDoListManagerDataClient>();

        var toDoItemDtos = grpcClient.ReturnAllToDoItems();

        if (toDoItemDtos is null)
        {
            Console.WriteLine($"No additional toDoItems found, returning from {nameof(FetchNewEntitiesAsync)}");

            return;
        }

        var toDoItemRepository = serviceScope.ServiceProvider.GetRequiredService<IBaseRepository<ToDoItemDocument>>();

        var allToDoItems = await toDoItemRepository.GetAllAsync();

        var newEntities = toDoItemDtos
            .Where(toDoItemDto =>
                !allToDoItems.Exists(toDoDocument =>
                    toDoDocument.Title == toDoItemDto.Title &&
                    toDoDocument.Description == toDoItemDto.Description));

        if (!newEntities.Any())
        {
            return;
        }

        var toDoItems = newEntities.Adapt<List<ToDoItemDocument>>();

        await InsertNewDataAsync(toDoItemRepository, toDoItems);
    }

    private static async Task InsertNewDataAsync(IBaseRepository<ToDoItemDocument> toDoItemRepository, IEnumerable<ToDoItemDocument> toDoItems)
    {
        await toDoItemRepository.CreateManyAsync(toDoItems);
    }
}