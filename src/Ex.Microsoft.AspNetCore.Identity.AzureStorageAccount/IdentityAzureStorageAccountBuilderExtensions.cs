using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.AzureStorageAccount;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityAzureStorageAccountBuilderExtensions
    {
        public static IdentityBuilder AddAzureStorageAccountStores(this IdentityBuilder builder, string connectionString)
        {
            builder.Services.Configure<AzureStorageAccountOptions>(options =>
            {
                options.ConnectionString = connectionString;
            });

            // TODO add overloads to allow for generic IdentityRole and IdentityUser.
            builder.AddRoleStore<AzureStorageAccountRoleStore<IdentityRole>>();
            builder.AddUserStore<AzureStorageAccountUserStore<IdentityUser>>();
            return builder;
        }
    }
}
