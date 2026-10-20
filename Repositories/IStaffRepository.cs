using DSAS.Web.Models;

namespace DSAS.Web.Repositories;

public interface IStaffRepository
{
    Task<StaffDto?> GetByIdAsync(string accessToken, string staffId);
    Task<bool> InsertAsync(string accessToken, string staffId, string email);
}