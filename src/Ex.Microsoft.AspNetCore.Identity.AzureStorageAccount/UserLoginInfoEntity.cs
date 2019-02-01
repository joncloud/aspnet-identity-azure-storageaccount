namespace Microsoft.AspNetCore.Identity.AzureStorageAccount
{
    public class UserLoginInfoEntity
    {
        public UserLoginInfoEntity()
        { }
        public UserLoginInfoEntity(string loginProvider, string providerKey, string providerDisplayName)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            ProviderDisplayName = providerDisplayName;
        }
        public UserLoginInfoEntity(UserLoginInfo userLoginInfo)
        {
            LoginProvider = userLoginInfo.LoginProvider;
            ProviderKey = userLoginInfo.ProviderKey;
            ProviderDisplayName = userLoginInfo.ProviderDisplayName;
        }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }

        public UserLoginInfo ToUserLoginInfo() =>
            new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);
    }
}
