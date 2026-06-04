using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListManager.Business.Contracts;
using ToDoListManager.Common.Dtos;

namespace ToDoListManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/to-do-lists")]
public class ToDoListController : ControllerBase
{
    private readonly IToDoListBusiness _toDoListBusiness;

    public ToDoListController(IToDoListBusiness toDoListBusiness)
    {
        _toDoListBusiness = toDoListBusiness;
    }

    [HttpGet]
    [Route("{toDoListGuid:guid}")]
    public async Task<ActionResult<CustomResponse<ToDoListDto?>>> GetToDoListByGuidAsync([FromRoute] Guid toDoListGuid, CancellationToken cancellationToken)
    {
        var result = await _toDoListBusiness.GetToDoListByGuidAsync(toDoListGuid, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    public async Task<ActionResult<CustomResponse<List<ToDoListDto>?>>> GetLoggedInUserToDoListsAsync(
        [FromQuery] PaginationDto? paginationDto,
        CancellationToken cancellationToken)
    {
        var result = await _toDoListBusiness.GetLoggedInUserToDoListsAsync(paginationDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomResponse<ToDoListDto?>>> CreateToDoListAsync(
        CreateOrUpdateToDoListDto createOrUpdateToDoListDto,
        CancellationToken cancellationToken)
    {
        var result = await _toDoListBusiness
            .CreateToDoListAsync(
                createOrUpdateToDoListDto,
                cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut]
    [Route("{toDoListGuid:guid}")]
    public async Task<ActionResult<CustomResponse<ToDoListDto?>>> UpdateToDoListAsync(
        Guid toDoListGuid,
        CreateOrUpdateToDoListDto createOrUpdateToDoListDto,
        CancellationToken cancellationToken)
    {
        var result = await _toDoListBusiness
            .UpdateToDoListAsync(
                toDoListGuid,
                createOrUpdateToDoListDto,
                cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{toDoListGuid:guid}")]
    public async Task<ActionResult<CustomResponse<ToDoListDto?>>> DeleteToDoListByGuidAsync(Guid toDoListGuid, CancellationToken cancellationToken)
    {
        var result = await _toDoListBusiness.DeleteToDoListByGuidAsync(toDoListGuid, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }
}