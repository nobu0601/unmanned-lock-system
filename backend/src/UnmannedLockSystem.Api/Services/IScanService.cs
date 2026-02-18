using UnmannedLockSystem.Api.Models.Dtos;

namespace UnmannedLockSystem.Api.Services;

public interface IScanService
{
    Task<ScanResponse> ProcessScanAsync(string qrToken);
}
