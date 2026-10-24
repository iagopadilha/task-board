namespace TaskBoard.Api.Dtos;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string? SignInKey { get; set; } // symetric key - validates if the content was not changed
    public string? Issuer { get; set; }
    public string? Audience { get; set; }

}
