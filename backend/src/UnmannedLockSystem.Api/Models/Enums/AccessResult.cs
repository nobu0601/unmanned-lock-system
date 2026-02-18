namespace UnmannedLockSystem.Api.Models.Enums;

public enum AccessResult
{
    Granted,
    DeniedExpired,
    DeniedUsed,
    DeniedInvalid,
    DeniedSignature,
    DeniedDoorMismatch
}
