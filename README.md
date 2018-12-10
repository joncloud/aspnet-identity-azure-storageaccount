# Ex.Microsoft.AspNetCore.Identity.AzureStorageAccount
[![NuGet](https://img.shields.io/nuget/v/Ex.Microsoft.AspNetCore.Identity.AzureStorageAccount.svg)](https://www.nuget.org/packages/Ex.Microsoft.AspNetCore.Identity.AzureStorageAccount/)

## Description
Ex.Microsoft.AspNetCore.Identity.AzureStorageAccount implements the ability to manage ASP.NET Core Identity using Azure Storage Accounts.

## Licensing
Released under the MIT License. See the [LICENSE][] File for further details.

[license]: LICENSE.md

## Usage
```csharp
public class Startup {
  public void ConfigureServices(IServiceCollection services) {
    var connectionString = "UseDevelopmentStorage=true"; // Swap out for production connection string
    services.AddDefaultIdentity<IdentityUser>()
      .AddRoles<IdentityRole>()
      .AddAzureStorageAccountStores(connectionString);
  }
}
```
