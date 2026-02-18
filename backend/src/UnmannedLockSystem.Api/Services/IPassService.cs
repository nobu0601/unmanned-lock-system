using UnmannedLockSystem.Api.Models.Dtos;
using UnmannedLockSystem.Api.Models.Entities;

namespace UnmannedLockSystem.Api.Services;

public interface IPassService
{
    Task<List<PassDto>> GetPassesByUserAsync(Guid userId);
    Task<List<PassDto>> GetAllPassesAsync(Guid? storeId = null);
    Task<AccessPass?> GetPassByIdAsync(Guid passId);
    Task<AccessPass> CreatePassFromOrderAsync(Order order);
    Task<bool> RevokePassAsync(Guid passId);
}
