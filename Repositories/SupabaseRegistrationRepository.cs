using Microsoft.Extensions.Options;
using DSAS.Web.Models;
using DSAS.Web.Repositories;

namespace DSAS_Web.Repositories
{
    public class SupabaseRegistrationRepository : SupabaseRepositoryBase, IRegistrationRepository
    {
        public SupabaseRegistrationRepository(HttpClient http, IOptions<SupabaseOptions> options)
            : base(http, options)
        {
        }

        public async Task<List<RegistrationDto>> GetAllAsync(string accessToken, string? search = null, string? status = null)
        {
            // We use "select=*,students(*)" to join the student table so the nested StudentDto is populated
            var request = NewRequest(HttpMethod.Get, "registration?select=*,students(*)", accessToken);
            var response = await Http.SendAsync(request);

            if (!response.IsSuccessStatusCode) return new List<RegistrationDto>();

            var list = await response.Content.ReadFromJsonAsync<List<RegistrationDto>>() ?? new List<RegistrationDto>();

            // Apply search filter using the NESTED Student object
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(r =>
                    (r.Student != null && r.Student.StudentNumber != null && r.Student.StudentNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Student != null && r.Student.FirstName != null && r.Student.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                list = list.Where(r => r.Status == status).ToList();
            }

            return list;
        }

        public async Task<RegistrationDto?> GetByIdAsync(string accessToken, string registrationId)
        {
            var request = NewRequest(HttpMethod.Get, $"registration?registration_id=eq.{registrationId}&select=*,students(*)", accessToken);
            var response = await Http.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var list = await response.Content.ReadFromJsonAsync<List<RegistrationDto>>();
            return list?.FirstOrDefault();
        }

        public async Task<bool> UpdateStatusAsync(string accessToken, string registrationId, string status, string? rejectionReason, string staffId)
        {
            var request = NewRequest(HttpMethod.Patch, $"registration?registration_id=eq.{registrationId}", accessToken);

            // Note: We use the property names from your RegistrationDto (rejection_reason)
            request.Content = JsonContent.Create(new
            {
                status = status,
                rejection_reason = rejectionReason,
                staff_id = staffId
            });

            var response = await Http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string accessToken, string registrationId)
        {
            var request = NewRequest(HttpMethod.Delete, $"registration?registration_id=eq.{registrationId}", accessToken);
            var response = await Http.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
