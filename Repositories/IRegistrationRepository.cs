using DSAS.Web.Models;

namespace DSAS.Web.Repositories;

public interface IRegistrationRepository
{
    Task<List<RegistrationDto>> GetAllAsync(string accessToken, string? search = null, string? status = null);
    Task<RegistrationDto?> GetByIdAsync(string accessToken, string registrationId);
    Task<bool> UpdateStatusAsync(string accessToken, string registrationId, string status, string? rejectionReason, string staffId);
    Task<bool> DeleteAsync(string accessToken, string registrationId);
}