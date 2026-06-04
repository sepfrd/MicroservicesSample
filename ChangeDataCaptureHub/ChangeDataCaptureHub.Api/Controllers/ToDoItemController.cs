using ChangeDataCaptureHub.Business.Businesses;
using ChangeDataCaptureHub.ExternalService.ToDoListManager;
using ChangeDataCaptureHub.Model.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace ChangeDataCaptureHub.Api.Controllers;

[Route("to-do-items")]
public class ToDoItemController : BaseController<ToDoItemDocument>
{
    private readonly ToDoItemBusiness _toDoItemBusiness;
    private readonly ToDoListManagerRestService _toDoListManagerRestService;

    public ToDoItemController(ToDoItemBusiness toDoItemBusiness, ToDoListManagerRestService toDoListManagerRestService)
        : base(toDoItemBusiness)
    {
        _toDoItemBusiness = toDoItemBusiness;
        _toDoListManagerRestService = toDoListManagerRestService;
    }

    [HttpGet]
    [Route("all-to-do-items-import")]
    public async Task<IActionResult> ImportToDoItemsAsync(CancellationToken cancellationToken)
    {
        var toDoItems = await _toDoListManagerRestService.GetToDoItemsAsync(cancellationToken);

        if (toDoItems is null || toDoItems.Count == 0)
        {
            return NoContent();
        }

        var allToDoItems = await _toDoItemBusiness.GetAllAsync(cancellationToken);

        var newEntities = toDoItems
            .Where(toDoItem =>
                !allToDoItems.Exists(toDoDocument =>
                    toDoDocument.Title == toDoItem.Title &&
                    toDoDocument.Description == toDoItem.Description));

        if (!newEntities.Any())
        {
            return NoContent();
        }

        var newToDoItems = newEntities.Adapt<List<ToDoItemDocument>>();

        await _toDoItemBusiness.CreateManyAsync(newToDoItems, cancellationToken);

        return Ok();
    }
}