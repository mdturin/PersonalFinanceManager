namespace PersonalFinanceManager.Application.DTOs.Alert;

public class AlertDto
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string Severity { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Source { get; set; }
}