using Azure.Identity;
using Azure.Messaging.EventHubs.Producer;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using System;

namespace ManagedIdentities;

public static class Program
{
    private const string UserAssignedClientId = "[USER-ASSIGNED-CLIENT-ID]";

    private static readonly Uri _uri = new("https://random-vault.vault.azure.net/");

    /// <summary>
    /// In order to get an access token programmatically, we can go various routes which will be displayed below.
    /// </summary>
    public static void Main(string[] args)
    {
        // The first route is to define the DefaultAzureCredential without any additional parameters.
        // E.g.; Create a KeyVault SecretClient instance using the DefaultAzureCredential.
        // Example uses: Azure.Security.KeyVault.Secrets
        _ = new SecretClient(new Uri("https://myvault.vault.azure.net/"), new DefaultAzureCredential());

        // The second route is to define the DefaultAzureCredential with a specific user-assigned managed identity.
        // E.g.; When deployed to an Azure host. The default Azure credential will authenticate the specified user-assigned managed identity.
        // Example uses: Azure.Storage.Blobs
        _ = new BlobClient(
            new Uri("https://myaccount.blob.core.windows.net/mycontainer/myblob"),
            new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = UserAssignedClientId
                }
            )
        );

        // The third route is to define a custom authentication flow with ChainedTokenCredential.
        // E.g.; Authenticate using managed identity if it is available; otherwise use the Azure CLI to authenticate.
        // Example uses: Azure.Messaging.EventHubs
        _ = new EventHubProducerClient(
            "myeventhub.eventhubs.windows.net",
            "myhubpath",
            new ChainedTokenCredential(
                new ManagedIdentityCredential(), new AzureCliCredential()
            )
        );
    }
}