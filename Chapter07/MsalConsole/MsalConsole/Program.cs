using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MsalConsole;

public static class Program
{
    /// <summary>
    /// Application (client) ID of the Entra/AAD app registration.
    /// </summary>
    private const string ApplicationClientId = "0d73f2a6-58e0-4c5d-a23b-989c5b901eaa";
    /// <summary>
    /// Directory (tenant) ID of the Entra/AAD app registration.
    /// </summary>
    private const string DirectoryTenantId = "14542c46-9768-49e7-92de-824664cfa058";
    /// <summary>
    /// Set of permission scopes that are assigned to the Entra/AAD app registration.
    /// </summary>
    private static readonly string[] PermissionScopes = [ "User.Read", "Calendars.Read" ];

    private static async Task Main(string[] args)
    {
        var publicClientApplication = PublicClientApplicationBuilder
            .Create(ApplicationClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, DirectoryTenantId)
            .WithRedirectUri("http://localhost")
            .Build();

        await SetupCacheFolder(publicClientApplication);

        var authenticationResult = await ObtainToken(publicClientApplication);
        if (authenticationResult != null)
        {
            Console.WriteLine($"Id: {authenticationResult.IdToken}");
            Console.WriteLine($"Access: {authenticationResult.AccessToken}");
        }
    }

    private static async Task SetupCacheFolder(
        IPublicClientApplication publicClientApplication
    )
    {
        var cacheFileName = "users.cache";
        var cacheFileLocation = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Cache"
        );
        if (!Directory.Exists(cacheFileLocation))
            _ = Directory.CreateDirectory(cacheFileLocation);

        Console.WriteLine($"Storing cache to directory {cacheFileLocation}");
        var storageProperties = new StorageCreationPropertiesBuilder(
                cacheFileName, cacheFileLocation
            )
            .Build();
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        cacheHelper.RegisterCache(publicClientApplication.UserTokenCache);
    }

    private static async Task<AuthenticationResult?> ObtainToken(
        IPublicClientApplication publicClientApplication
    )
    {
        var accounts = await publicClientApplication.GetAccountsAsync();

        try
        {
            return await ObtainTokenSilent(publicClientApplication, accounts);
        }
        catch(MsalUiRequiredException)
        {
            try
            {
                return await ObtainTokenInteractive(publicClientApplication);
            }
            // Catch MSAL exceptions
            catch(MsalException ex)
            {
                Console.WriteLine($"Failed to get token interactively: {Environment.NewLine}{ex.Message}");
            }
        }
        catch(MsalException ex)
        {
            Console.WriteLine($"Failed to get token silently: {Environment.NewLine}{ex.Message}");
        }

        return null;
    }

    private static async Task<AuthenticationResult> ObtainTokenSilent(
        IPublicClientApplication publicClientApplication, IEnumerable<IAccount> accounts
    )
    {
        Console.WriteLine("Trying to get token from cache...");
        var authenticationResult = await publicClientApplication.AcquireTokenSilent(
                PermissionScopes, accounts.FirstOrDefault()
            )
            .ExecuteAsync();
        Console.WriteLine("Token acquired from cache successfully.");
        return authenticationResult;
    }

    private static async Task<AuthenticationResult> ObtainTokenInteractive(
        IPublicClientApplication publicClientApplication
    )
    {
        Console.WriteLine("Trying to get token interactively...");
        var authenticationResult = await publicClientApplication
            .AcquireTokenInteractive(PermissionScopes)
            .ExecuteAsync();
        Console.WriteLine("Token acquired successfully.");
        return authenticationResult;
    }
}