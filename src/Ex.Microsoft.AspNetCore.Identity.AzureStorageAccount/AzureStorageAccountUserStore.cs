﻿using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.AzureStorageAccount
{
    public class AzureStorageAccountUserStore<TUser> : IUserStore<TUser>
        , IUserLoginStore<TUser>
        , IUserClaimStore<TUser>
        , IUserPasswordStore<TUser>
        , IUserSecurityStampStore<TUser>
        , IUserEmailStore<TUser>
        , IUserLockoutStore<TUser>
        , IUserPhoneNumberStore<TUser>
        , IUserTwoFactorStore<TUser>
        , IUserAuthenticationTokenStore<TUser>
        , IUserAuthenticatorKeyStore<TUser>
        , IUserTwoFactorRecoveryCodeStore<TUser>
        , IProtectedUserStore<TUser>
        where TUser : IdentityUser
    {
        static class TableNames
        {
            public static class User
            {
                public const string ById = "AspNetUsersById";
                public const string ByUserName = "AspNetUsersByUserName";
                public const string ByLoginProvider = "AspNetUsersByLoginProvider";
                public const string ByEmailAddress = "AspNetUsersByEmailAddress";
                public const string ByClaim = "AspNetUsersByClaim";
            }

            public static class UserLogins
            {
                public const string ByUserId = "AspNetUserLoginsByUserId";
            }

            public static class Claims
            {
                public const string ByUserId = "AspNetClaimsByUserId";
            }

            public static class UserTokens
            {
                public const string ByUserIdLoginProviderName = "AspNetUserTokensByUserIdLoginProviderName";
            }
        }

        CloudStorageAccount _account;
        public AzureStorageAccountUserStore(IOptions<AzureStorageAccountOptions> options)
        {
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            await tableClient.InsertAsync(TableNames.User.ById, user, id, id);

            var normalizedUserName = await GetNormalizedUserNameAsync(user, cancellationToken);
            await tableClient.InsertAsync(TableNames.User.ByUserName, user, normalizedUserName, normalizedUserName);

            var emailAddress = await GetEmailAsync(user, cancellationToken);
            await tableClient.InsertAsync(TableNames.User.ByEmailAddress, user, emailAddress, emailAddress);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            await tableClient.DeleteAsync(TableNames.User.ById, user, id, id);

            var normalizedUserName = await GetNormalizedUserNameAsync(user, cancellationToken);
            await tableClient.DeleteAsync(TableNames.User.ByUserName, user, normalizedUserName, normalizedUserName);

            var normalizedEmailAddress = await GetNormalizedEmailAsync(user, cancellationToken);
            await tableClient.DeleteAsync(TableNames.User.ByEmailAddress, user, normalizedEmailAddress, normalizedEmailAddress);

            return IdentityResult.Success;
        }

        public void Dispose() { }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();
            return await tableClient.FindAsync<TUser>(TableNames.User.ById, userId, userId);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();
            return await tableClient.FindAsync<TUser>(TableNames.User.ByUserName, normalizedUserName, normalizedUserName);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedUserName);

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id);

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.UserName);

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            await tableClient.InsertOrReplaceAsync(TableNames.User.ById, user, id, id);

            var normalizedUserName = await GetNormalizedUserNameAsync(user, cancellationToken);
            await tableClient.InsertOrReplaceAsync(TableNames.User.ByUserName, user, normalizedUserName, normalizedUserName);

            var normalizedEmailAddress = await GetNormalizedEmailAsync(user, cancellationToken);
            await tableClient.InsertOrReplaceAsync(TableNames.User.ByEmailAddress, user, normalizedEmailAddress, normalizedEmailAddress);

            return IdentityResult.Success;
        }

        public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            await tableClient.InsertAsync(TableNames.User.ByLoginProvider, user, login.LoginProvider, login.ProviderKey);

            await tableClient.InsertAsync(TableNames.UserLogins.ByUserId, login, user.Id, login.LoginProvider);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            await tableClient.DeleteAsync(TableNames.User.ByLoginProvider, user, loginProvider, providerKey);

            var login = new UserLoginInfo(loginProvider, providerKey, loginProvider);
            await tableClient.DeleteAsync(TableNames.UserLogins.ByUserId, login, user.Id, loginProvider);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            return await tableClient.FindAllAsync<UserLoginInfo>(TableNames.User.ByLoginProvider, id);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            return await tableClient.FindAsync<TUser>(TableNames.User.ByLoginProvider, loginProvider, providerKey);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            return await tableClient.FindAllAsync<Claim>(TableNames.Claims.ByUserId, id);
        }

        public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);

            await tableClient.InsertAsync(TableNames.Claims.ByUserId, claims, id, claim => claim.GetKey());

            foreach (var claim in claims)
            {
                await tableClient.InsertAsync(TableNames.User.ByClaim, user, claim.GetKey(), id);
            }
        }

        public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            var existingKey = claim.GetKey();
            var existingClaim = await tableClient.FindAsync<Claim>(TableNames.Claims.ByUserId, id, existingKey);
            if (existingClaim != null)
            {
                await tableClient.DeleteAsync(TableNames.Claims.ByUserId, existingClaim, id, existingKey);
            }
            var existingUser = await tableClient.FindAsync<TUser>(TableNames.User.ByClaim, existingKey, id);
            if (existingUser != null)
            {
                await tableClient.DeleteAsync(TableNames.User.ByClaim, existingUser, existingKey, id);
            }

            var newKey = newClaim.GetKey();
            await tableClient.InsertOrReplaceAsync(TableNames.Claims.ByUserId, newClaim, id, newKey);
            await tableClient.InsertOrReplaceAsync(TableNames.User.ByClaim, user, newKey, id);
        }

        public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var id = await GetUserIdAsync(user, cancellationToken);
            await tableClient.DeleteAsync(TableNames.Claims.ByUserId, claims, id, claim => claim.GetKey());

            foreach (var claim in claims)
            {
                await tableClient.DeleteAsync(TableNames.User.ByClaim, user, claim.GetKey(), id);
            }
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            var partitionKey = claim.GetKey();
            return await tableClient.FindAllAsync<TUser>(TableNames.User.ByClaim, partitionKey);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash != null);

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.SecurityStamp);

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var tableClient = _account.CreateCloudTableClient();

            return await tableClient.FindAsync<TUser>(TableNames.User.ByEmailAddress, normalizedEmail, normalizedEmail);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.LockoutEnd);

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(++user.AccessFailedCount);

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.AccessFailedCount);

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.LockoutEnabled);

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PhoneNumber);

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PhoneNumberConfirmed);

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.TwoFactorEnabled);

        public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            var partitionKey = await GetUserIdAsync(user, cancellationToken);
            var rowKey = string.Join(":", loginProvider, name);

            var tableClient = _account.CreateCloudTableClient();

            await tableClient.InsertOrReplaceAsync(TableNames.UserTokens.ByUserIdLoginProviderName, value, partitionKey, rowKey);
        }

        public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var partitionKey = await GetUserIdAsync(user, cancellationToken);
            var rowKey = string.Join(":", loginProvider, name);

            var tableClient = _account.CreateCloudTableClient();

            await tableClient.DeleteAsync(TableNames.UserTokens.ByUserIdLoginProviderName, "", partitionKey, rowKey);
        }

        public async Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var partitionKey = await GetUserIdAsync(user, cancellationToken);
            var rowKey = string.Join(":", loginProvider, name);

            var tableClient = _account.CreateCloudTableClient();

            return await tableClient.FindAsync<string>(TableNames.UserTokens.ByUserIdLoginProviderName, partitionKey, rowKey);
        }

        const string InternalLoginProvider = "[AspNetUserStore]";
        const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        const string RecoveryCodeTokenName = "RecoveryCodes";
        public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken) =>
            SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);

        public Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken) =>
            GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);

        public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            var mergedCodes = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
        }

        public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }
            return 0;
        }
    }
}
