using Microsoft.Extensions.Logging;

namespace UnmannedLockSystem.Api.Adapters.SmartLock;

public class MockSmartLockAdapter : ISmartLockAdapter
{
    private readonly ILogger<MockSmartLockAdapter> _logger;

    public MockSmartLockAdapter(ILogger<MockSmartLockAdapter> logger)
    {
        _logger = logger;
    }

    public Task<SmartLockResult> UnlockAsync(Guid doorId, Guid passId, string reason)
    {
        _logger.LogInformation("[MockSmartLock] UNLOCK door {DoorId} for pass {PassId}. Reason: {Reason}",
            doorId, passId, reason);
        return Task.FromResult(new SmartLockResult { Success = true });
    }

    public Task<SmartLockResult> LockAsync(Guid doorId, string reason)
    {
        _logger.LogInformation("[MockSmartLock] LOCK door {DoorId}. Reason: {Reason}", doorId, reason);
        return Task.FromResult(new SmartLockResult { Success = true });
    }

    public Task<bool> GetStatusAsync(Guid doorId)
    {
        _logger.LogInformation("[MockSmartLock] GET STATUS door {DoorId} -> locked", doorId);
        return Task.FromResult(true);
    }
}
