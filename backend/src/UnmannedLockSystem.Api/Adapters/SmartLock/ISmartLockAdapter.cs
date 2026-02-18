namespace UnmannedLockSystem.Api.Adapters.SmartLock;

public class SmartLockResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface ISmartLockAdapter
{
    Task<SmartLockResult> UnlockAsync(Guid doorId, Guid passId, string reason);
    Task<SmartLockResult> LockAsync(Guid doorId, string reason);
    Task<bool> GetStatusAsync(Guid doorId);
}
