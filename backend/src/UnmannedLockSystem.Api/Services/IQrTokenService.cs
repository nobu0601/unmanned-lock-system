using UnmannedLockSystem.Api.Models.Dtos;

namespace UnmannedLockSystem.Api.Services;

public class QrTokenClaims
{
    public Guid PassId { get; set; }
    public Guid DoorId { get; set; }
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
}

public interface IQrTokenService
{
    QrTokenResponse GenerateToken(Guid passId, Guid doorId, Guid storeId, Guid userId);
    QrTokenClaims? ValidateToken(string token);
}
