using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DSAS.Web.Models;
using Microsoft.Extensions.Options;

namespace DSAS.Web.Repositories;

// Talks to Supabase Auth (GoTrue) over REST -- the same endpoint the
// Flutter app's Supabase client calls under the hood, so both apps
// authenticate against the one shared user pool. "Confirm email" is
// assumed OFF in the Supabase dashboard, so SignUp always returns a
// usable access token immediately -- no "check your email" step.
public class SupabaseAuthRepository : IAuthRepository
{
    private readonly HttpClient _http;
    private readonly ILogger<SupabaseAuthRepository> _logger;

    public SupabaseAuthRepository(HttpClient http, IOptions<SupabaseOptions> options, ILogger<SupabaseAuthRepository> logger)
    {
        var config = options.Value;
        http.BaseAddress = new Uri(config.Url);
        http.DefaultRequestHeaders.Add("apikey", config.AnonKey);
        _http = http;
        _logger = logger;
    }

    public async Task<AuthResult> SignInAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/auth/v1/token?grant_type=password", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<GoTrueError>();
                return new AuthResult(false, null, null, error?.ErrorDescription ?? error?.Msg ?? "Invalid email or password.");
            }

            var token = await response.Content.ReadFromJsonAsync<GoTrueTokenResponse>();
            return new AuthResult(true, token?.AccessToken, token?.User?.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign-in request to Supabase Auth failed.");
            return new AuthResult(false, null, null, "Could not reach the authentication service. Please try again.");
        }
    }

    public async Task<AuthResult> SignUpAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/auth/v1/signup", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<GoTrueError>();
                return new AuthResult(false, null, null, error?.ErrorDescription ?? error?.Msg ?? "Could not create account.");
            }

            var token = await response.Content.ReadFromJsonAsync<GoTrueTokenResponse>();
            return new AuthResult(true, token?.AccessToken, token?.User?.Id, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign-up request to Supabase Auth failed.");
            return new AuthResult(false, null, null, "Could not reach the authentication service. Please try again.");
        }
    }

    private class GoTrueTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("user")]
        public GoTrueUser? User { get; set; }
    }

    private class GoTrueUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class GoTrueError
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }
}