using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Helper
{
    public static class IdentityHelper
    {
        public static List<string> GetIdentityResultErrorDescriptions(IdentityResult identityResult)
        {
            var errorDescriptions = new List<string>();

            foreach (var error in identityResult.Errors)
            {
                errorDescriptions.Add(error.Description);
            }

            return errorDescriptions;
        }
    }
}
