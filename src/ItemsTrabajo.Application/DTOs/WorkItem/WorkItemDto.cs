namespace ItemsTrabajo.Application.DTOs.WorkItem;

public class WorkItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Relevance { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
}