namespace ToDoListManager.Common.Dtos;

public record CreateOrUpdateToDoListDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}