using System.Security.Claims;

namespace VanSalesAPI.Helpers
{
    public static class JwtExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst("id")?.Value;
            return idClaim == null ? 0 : int.Parse(idClaim);
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst("role")?.Value ?? "";
        }
    }
}