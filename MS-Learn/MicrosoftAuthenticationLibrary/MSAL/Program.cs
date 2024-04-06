using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace MSAL;

public static class Program
{
    private const string ClientId = "55bfa9ad-ba71-40f7-8863-4a99ca996dce";
    private const string TenantId = "14542c46-9768-49e7-92de-824664cfa058";
    
    private static async Task Main(string[] args)
    {
        var app = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
            .WithRedirectUri("http://localhost")
            .Build();
        
        string[] scopes = ["user.read"];

        var authenticationResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
        
        Console.WriteLine($"Token: {authenticationResult.AccessToken}");
    }
}