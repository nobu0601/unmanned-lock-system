using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnmannedLockSystem.Api.Adapters.SmartLock;
using UnmannedLockSystem.Api.Data;
using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Models.Entities;
using UnmannedLockSystem.Api.Models.Enums;

namespace UnmannedLockSystem.Api.Services;

public class ScanService : IScanService
{
    private readonly AppDbContext _db;
    private readonly IQrTokenService _qrTokenService;
    private readonly ISmartLockAdapter _smartLock;
    private readonly ILogger<ScanService> _logger;

    public ScanService(
        AppDbContext db,
        IQrTokenService qrTokenService,
        ISmartLockAdapter smartLock,
        ILogger<ScanService> logger)
    {
        _db = db;
        _qrTokenService = qrTokenService;
        _smartLock = smartLock;
        _logger = logger;
    }

    public async Task<ScanResponse> ProcessScanAsync(string qrToken)
    {
        // 1. Validate JWT signature and expiration
        var claims = _qrTokenService.ValidateToken(qrToken);
        if (claims == null)
        {
            _logger.LogWarning("Scan denied: invalid or expired QR token");
            return new ScanResponse
            {
                Granted = false,
                Message = "Invalid or expired QR code",
                DenialReason = "invalid_signature_or_expired"
            };
        }

        // 2. Transaction-based pass validation
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var pass = await _db.AccessPasses
                .FirstOrDefaultAsync(p => p.Id == claims.PassId);

            if (pass == null)
            {
                await LogAccessAsync(claims, AccessResult.DeniedInvalid, "Pass not found");
                await transaction.CommitAsync();
                return Denied("Pass not found", "pass_not_found");
            }

            // Check pass status
            if (pass.Status != PassStatus.Active)
            {
                await LogAccessAsync(claims, AccessResult.DeniedInvalid, $"Pass status: {pass.Status}");
                await transaction.CommitAsync();
                return Denied($"Pass is {pass.Status}", "pass_not_active");
            }

            // Check used count
            if (pass.UsedCount >= pass.MaxUses)
            {
                await LogAccessAsync(claims, AccessResult.DeniedUsed, "Max uses exceeded");
                await transaction.CommitAsync();
                return Denied("Pass already used", "already_used");
            }

            // Check validity period
            var now = DateTime.UtcNow;
            if (now < pass.ValidFrom || now > pass.ValidTo)
            {
                await LogAccessAsync(claims, AccessResult.DeniedExpired, $"Valid: {pass.ValidFrom} - {pass.ValidTo}");
                await transaction.CommitAsync();
                return Denied("Pass not valid at this time", "time_expired");
            }

            // Check door match
            if (pass.DoorId != claims.DoorId)
            {
                await LogAccessAsync(claims, AccessResult.DeniedDoorMismatch, $"Expected: {pass.DoorId}, Got: {claims.DoorId}");
                await transaction.CommitAsync();
                return Denied("Door mismatch", "door_mismatch");
            }

            // All checks passed - update pass
            pass.UsedCount++;
            if (pass.UsedCount >= pass.MaxUses)
            {
                pass.Status = PassStatus.Used;
            }
            pass.UpdatedAt = DateTime.UtcNow;

            // Log access granted
            await LogAccessAsync(claims, AccessResult.Granted, null);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // 3. Unlock door AFTER commit (fail-safe)
            try
            {
                var unlockResult = await _smartLock.UnlockAsync(claims.DoorId, claims.PassId, "QR scan access granted");
                if (!unlockResult.Success)
                {
                    _logger.LogError("SmartLock unlock failed for door {DoorId}: {Error}",
                        claims.DoorId, unlockResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SmartLock unlock exception for door {DoorId}", claims.DoorId);
            }

            return new ScanResponse
            {
                Granted = true,
                Message = "Access granted"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Scan processing error for pass {PassId}", claims.PassId);
            return Denied("Internal error", "internal_error");
        }
    }

    private async Task LogAccessAsync(QrTokenClaims claims, AccessResult result, string? reason)
    {
        _db.AccessLogs.Add(new AccessLog
        {
            Id = Guid.NewGuid(),
            AccessPassId = claims.PassId,
            StoreId = claims.StoreId,
            DoorId = claims.DoorId,
            Result = result,
            DenialReason = reason,
            ScannedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private static ScanResponse Denied(string message, string reason)
    {
        return new ScanResponse
        {
            Granted = false,
            Message = message,
            DenialReason = reason
        };
    }
}
