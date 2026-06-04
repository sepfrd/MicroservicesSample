using Grpc.Core;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ToDoListManager.Common.Dtos;
using ToDoListManager.DataAccess.Contracts;
using ToDoListManager.DataAccess.Repositories;

namespace ToDoListManager.Services.Grpc;

public class GrpcService : ToDoListManagerGrpc.ToDoListManagerGrpcBase
{
    private readonly ToDoItemRepository _toDoItemRepository;

    public GrpcService(IServiceScopeFactory serviceScopeFactory)
    {
        var unitOfWork = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();

        _toDoItemRepository = (ToDoItemRepository)unitOfWork.ToDoItemRepository;
    }

    public override async Task<GrpcResponse?> GetAllToDoItems(GetAllRequest request, ServerCallContext context)
    {
        var response = new GrpcResponse();

        var toDoItems = await _toDoItemRepository.GetAllToDoItemsWithoutPaginationAsync();

        var toDoItemDtos = toDoItems.Adapt<List<ToDoItemDto>>();

        var grpcToDoItemModels = toDoItemDtos.Adapt<List<GrpcToDoItemModel>>();

        response.ToDoItem.AddRange(grpcToDoItemModels);

        return response;
    }
}