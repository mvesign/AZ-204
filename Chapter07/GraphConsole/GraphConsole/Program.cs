using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GraphConsole;

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
    private static readonly string[] PermissionScopes = [ "User.Read" ];

    private static async Task Main(string[] args)
    {
        var publicClientApplication = PublicClientApplicationBuilder
            .Create(ApplicationClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, DirectoryTenantId)
            .WithRedirectUri("http://localhost")
            .Build();

        var authProvider = new DelegateAuthenticationProvider(async (request) =>
        {
            try
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", await ObtainTokenInteractive(publicClientApplication)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });

        var graphClient = new GraphServiceClient(authProvider);

        var user = await graphClient.Me.GetAsync();
        if (user != null)
            Console.WriteLine($"Hello, {user.GivenName}! Your name was obtained from MS Graph."); 
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