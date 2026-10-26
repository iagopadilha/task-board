using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TaskBoard.Api.Dtos;

namespace TaskBoard.Api.Services;

public interface ITokenGeneratorService
{
    string CreateJwtToken(Guid id, string name, string email);
}
// should always aim to use the web version of everything in jwt - way lighter
public class TokenGeneratorService(IOptions<JwtSettings> jwtOptions) : ITokenGeneratorService // this will be the token generator class
{
    private readonly JwtSettings _jwtSettingsValue = jwtOptions.Value;
    
    public string CreateJwtToken(Guid id, string name, string email)
    {
        var securityKeyFinal = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsValue.SignInKey!));
        var credentials = new SigningCredentials(securityKeyFinal, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity
            ([
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Name, name),
                new Claim(ClaimTypes.Role, "board_admin"), // we can also add many admins and many permissions
                new Claim("permission", "acess_board"),
                new Claim("permission", "manage_board"),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = credentials,
            Issuer = _jwtSettingsValue.Issuer,
            Audience = _jwtSettingsValue.Audience
        };

        var tokenHandler = new JsonWebTokenHandler();

        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
