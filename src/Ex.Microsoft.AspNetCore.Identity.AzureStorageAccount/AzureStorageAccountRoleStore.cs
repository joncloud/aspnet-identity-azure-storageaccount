using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.AzureStorageAccount
{
    public class AzureStorageAccountRoleStore<TRole> : IRoleStore<TRole>
        , IRoleClaimStore<TRole>
        where TRole : IdentityRole
    {
        static class TableNames
        {
            public static class Roles
            {
                public const string ById = "AspNetRolesById";
                public const string ByRoleName = "AspNetRolesByRoleName";
            }

            public static class Claims
            {
                public const string ByRoleId = "AspNetClaimsByRoleId";
            }
        }

        CloudStorageAccount _account;
        public AzureStorageAccountRoleStore(IOptions<AzureStorageAccountOptions> options)
        {
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            await tableClient.InsertAsync(TableNames.Roles.ById, role, id, id);

            var normalizedUserName = await GetNormalizedRoleNameAsync(role, cancellationToken);
            await tableClient.InsertAsync(TableNames.Roles.ByRoleName, role, normalizedUserName, normalizedUserName);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            await tableClient.DeleteAsync(TableNames.Roles.ById, role, id, id);

            var normalizedUserName = await GetNormalizedRoleNameAsync(role, cancellationToken);
            await tableClient.DeleteAsync(TableNames.Roles.ByRoleName, role, normalizedUserName, normalizedUserName);

            return IdentityResult.Success;
        }

        public void Dispose() { }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();
            return await tableClient.FindAsync<TRole>(TableNames.Roles.ById, roleId, roleId);
        }

        public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();
            return await tableClient.FindAsync<TRole>(TableNames.Roles.ByRoleName, normalizedRoleName, normalizedRoleName);
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken) =>
            Task.FromResult(role.NormalizedName);

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken) =>
            Task.FromResult(role.Id);

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken) =>
            Task.FromResult(role.Name);

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            await tableClient.InsertOrReplaceAsync(TableNames.Roles.ById, role, id, id);

            var normalizedRoleName = await GetNormalizedRoleNameAsync(role, cancellationToken);
            await tableClient.InsertOrReplaceAsync(TableNames.Roles.ByRoleName, role, normalizedRoleName, normalizedRoleName);

            return IdentityResult.Success;
        }

        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            return await tableClient.FindAllAsync<Claim>(TableNames.Claims.ByRoleId, id);
        }

        public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            await tableClient.InsertAsync(TableNames.Claims.ByRoleId, claim, id, claim.GetKey());
        }

        public async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetRoleIdAsync(role, cancellationToken);
            await tableClient.DeleteAsync(TableNames.Claims.ByRoleId, claim, id, claim.GetKey());
        }
    }
}
