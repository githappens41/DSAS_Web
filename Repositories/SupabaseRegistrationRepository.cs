using Microsoft.Extensions.Options;
using DSAS.Web.Models;
using DSAS.Web.Repositories;

namespace DSAS.Web.Repositories
{
    public class SupabaseRegistrationRepository : SupabaseRepositoryBase, IRegistrationRepository
    {
        public SupabaseRegistrationRepository(HttpClient http, IOptions<SupabaseOptions> options)
            : base(http, options)
        {
        }

        public async Task<List<RegistrationDto>> GetAllAsync(string accessToken, string? search = null, string? status = null)
        {
            var request = NewRequest(HttpMethod.Get, "student_cards?select=*", accessToken);
            var response = await Http.SendAsync(request);

            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Supabase Error: {response.StatusCode} - {error}");
            }

            var list = await response.Content.ReadFromJsonAsync<List<RegistrationDto>>() ?? new List<RegistrationDto>();

            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(r =>
                    (r.FullName != null && r.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (r.StudentNumber != null && r.StudentNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
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
            // It MUST look at 'student_cards' and the column 'id'
            var request = NewRequest(HttpMethod.Get, $"student_cards?id=eq.{registrationId}&select=*", accessToken);
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
