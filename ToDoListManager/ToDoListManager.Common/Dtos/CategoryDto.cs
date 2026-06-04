namespace ToDoListManager.Common.Dtos;

public record CategoryDto(Guid Guid, string Name, Guid UserGuid)
{
    public Guid Guid { get; set; }

    public string? Name { get; set; }

    public Guid UserGuid { get; set; }
}