namespace ToDoListManager.Common.Dtos;

public record CategoryDto
{
    public Guid Guid { get; set; }

    public string? Name { get; set; }

    public Guid UserGuid { get; set; }
}