namespace UnmannedLockSystem.Api.Models.Dtos;

public class ScanResponse
{
    public bool Granted { get; set; }
    public string Message { get; set; } = null!;
    public string? DenialReason { get; set; }
}
