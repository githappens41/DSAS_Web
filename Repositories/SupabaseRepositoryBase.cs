using System.Net.Http.Headers;
using DSAS.Web.Models;
using Microsoft.Extensions.Options;

namespace DSAS.Web.Repositories;

// Shared setup for any repository that talks to Supabase's PostgREST
// data API. Every request carries the SIGNED-IN staff member's own
// access token as the bearer token -- not a service-role key -- so Row
// Level Security is what actually enforces access, not this code.
public abstract class SupabaseRepositoryBase
{
    protected readonly HttpClient Http;
    protected readonly SupabaseOptions Options;

    protected SupabaseRepositoryBase(HttpClient http, IOptions<SupabaseOptions> options)
    {
        Options = options.Value;
        http.BaseAddress = new Uri($"{Options.Url}/rest/v1/");
        Http = http;
    }

    protected HttpRequestMessage NewRequest(HttpMethod method, string relativeUrl, string accessToken)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.Add("apikey", Options.AnonKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}