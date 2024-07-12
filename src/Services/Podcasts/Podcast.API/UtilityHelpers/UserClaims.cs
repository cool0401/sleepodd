using System.Security.Claims;

namespace Podcast.API.UtilityHelpers
{
    public static class UserClaims
    {
        public static bool TryValidate(ClaimsPrincipal claims, out User user, out string ErrorMessage)
        {
            if (!Guid.TryParse(claims.FindFirstValue("jti"), out var _))
                ErrorMessage = "Invalid `tokenId`: Not a valid GUID.";

            Guid UserId = Guid.Empty;
            string Username = "";
            user = null;
            ErrorMessage = "";

            var userIds = claims.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToArray();
            if (userIds.Length == 2) // only userid and username
            {
                if (Guid.TryParse(userIds[0].Value, out Guid userId0))
                {
                    UserId = userId0;
                    Username = userIds[1].Value;
                }
                else if (Guid.TryParse(userIds[1].Value, out Guid userId1))
                {
                    UserId = userId1;
                    Username = userIds[0].Value;
                }
                else
                {
                    ErrorMessage = "Invalid userid";
                    return false;
                }
            }
            else if (userIds.Length == 1) // only userid or only username
            {
                if (Guid.TryParse(userIds[0].Value, out Guid userId)) UserId = userId;
                else Username = userIds[0].Value;
            }
            else
            {
                ErrorMessage = "Invalid claims of type 'nameidentifier'";
                return false;
            }

            string FullName = claims.FindFirstValue(ClaimTypes.Name);
            string Email = claims.FindFirstValue(ClaimTypes.Email);
            string Role = claims.FindFirstValue(ClaimTypes.Role);

            user = new User
            {
                Id = UserId,
                UserName = Username,
                Name = FullName,
                Email = Email
            };

            return true;
        }
    }

}
