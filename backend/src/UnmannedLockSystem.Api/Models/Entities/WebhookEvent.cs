namespace UnmannedLockSystem.Api.Models.Entities;

public class WebhookEvent
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = "stripe";
    public string EventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string Status { get; set; } = "received";
    public string? Payload { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
