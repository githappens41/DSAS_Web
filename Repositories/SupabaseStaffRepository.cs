using System.Net.Http.Json;
using DSAS.Web.Models;
using Microsoft.Extensions.Options;

namespace DSAS.Web.Repositories;

public class SupabaseStaffRepository : SupabaseRepositoryBase, IStaffRepository
{
    private readonly ILogger<SupabaseStaffRepository> _logger;

    public SupabaseStaffRepository(HttpClient http, IOptions<SupabaseOptions> options, ILogger<SupabaseStaffRepository> logger)
        : base(http, options)
    {
        _logger = logger;
    }

    // staff_id IS the Supabase Auth user id directly (matches the ERD --
    // no separate authUserID column), so this is a lookup by primary key.
    public async Task<StaffDto?> GetByIdAsync(string accessToken, string staffId)
    {
        try
        {
            var request = NewRequest(HttpMethod.Get, $"staff?staff_id=eq.{staffId}&select=*", accessToken);
            var response = await Http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var results = await response.Content.ReadFromJsonAsync<List<StaffDto>>();
            return results?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to look up staff profile {StaffId}.", staffId);
            return null;
        }
    }

    public async Task<bool> InsertAsync(string accessToken, string staffId, string email)
    {
        try
        {
            var request = NewRequest(HttpMethod.Post, "staff", accessToken);
            request.Headers.Add("Prefer", "return=minimal");
            request.Content = JsonContent.Create(new { staff_id = staffId, email });
            var response = await Http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert staff profile {StaffId}.", staffId);
            return false;
        }
    }
}