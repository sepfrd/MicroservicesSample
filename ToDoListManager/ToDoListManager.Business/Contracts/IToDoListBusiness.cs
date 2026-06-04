using ToDoListManager.Common.Dtos;

namespace ToDoListManager.Business.Contracts;

public interface IToDoListBusiness
{
    Task<CustomResponse<ToDoListDto?>> GetToDoListByGuidAsync(Guid toDoListGuid, CancellationToken cancellationToken = default);

    Task<CustomResponse<List<ToDoListDto>?>> GetLoggedInUserToDoListsAsync(PaginationDto? paginationDto = null, CancellationToken cancellationToken = default);

    Task<CustomResponse<ToDoListDto?>> CreateToDoListAsync(CreateOrUpdateToDoListDto orUpdateToDoListDto, CancellationToken cancellationToken = default);

    Task<CustomResponse<ToDoListDto?>> UpdateToDoListAsync(Guid toDoListGuid, CreateOrUpdateToDoListDto createOrUpdateToDoListDto, CancellationToken cancellationToken = default);

    Task<CustomResponse<ToDoListDto?>> DeleteToDoListByGuidAsync(Guid toDoListGuid, CancellationToken cancellationToken = default);
}