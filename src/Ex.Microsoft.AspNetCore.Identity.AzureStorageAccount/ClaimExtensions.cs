using System.Security.Claims;

namespace Microsoft.AspNetCore.Identity.AzureStorageAccount
{
    static class ClaimExtensions
    {
        public static string GetRowKey(this Claim claim) =>
            claim.Type + claim.Value;
    }
}
