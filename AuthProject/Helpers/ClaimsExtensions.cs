using AuthProject.Dto;
using System.Security.Claims;

namespace AuthProject.Helpers
{
    public static class ClaimsExtensions
    {
        public static UserInfo ToUserInfo(this ClaimsPrincipal claimsPrincipal)
        {
            return new UserInfo
            {
                Username = claimsPrincipal.Identity.Name
            };
        }
    }
}
