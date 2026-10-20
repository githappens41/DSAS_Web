namespace DSAS.Web.Repositories;

public record AuthResult(bool Success, string? AccessToken, string? UserId, string? Error);

public interface IAuthRepository
{
    Task<AuthResult> SignInAsync(string email, string password);
    Task<AuthResult> SignUpAsync(string email, string password);
}