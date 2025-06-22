using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EMSMvc.Helpers
{
    public static class JwtHelper
    {
        public static Dictionary<string, string>? DecodeToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return null;

            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jsonToken == null) return null;

            var claimsDict = new Dictionary<string, string>();

            claimsDict["UserId"] = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
            claimsDict["UserName"] = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ??
                                     jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "";
            claimsDict["UserEmail"] = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ??
                                      jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";

            var roles = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            claimsDict["UserRole"] = roles.FirstOrDefault() ?? "";
            claimsDict["UserRoles"] = string.Join(",", roles);

            claimsDict["FullName"] = jsonToken.Claims.FirstOrDefault(c => c.Type == "full_name")?.Value ?? "";

            return claimsDict;
        }
    }
}
