namespace ItemsTrabajo.Application.DTOs.WorkItem;

public class CreateWorkItemDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Relevance { get; set; } = string.Empty;  // "H" o "L"
    public DateTime ExpirationDate { get; set; }
}