using ToDoListManager.Model.Enums;

namespace ToDoListManager.Common.Dtos;

public record CreateOrUpdateToDoItemDto
{
    public string? Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime DueDate { get; set; }

    public Priority Priority { get; set; }

    public Guid ToDoListGuid { get; set; }

    public Guid? CategoryGuid { get; set; }

    public string? Description { get; set; }
}