using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpTrigger(ILogger<HttpTrigger> logger)
{
    private readonly ILogger<HttpTrigger> _logger = logger;

    private const string UserAssignedClientId = "9e6ebcca-5ee3-40c6-a811-40ed6c9c4b59";
    private const string KeyVaultUri = "https://mjoy-kv.vault.azure.net/";

    [Function("HttpTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = UserAssignedClientId
            }
        );
        var client = new SecretClient(new Uri(KeyVaultUri), credential);

        var secret = await client.GetSecretAsync("SecretKey");

        return new OkObjectResult($"Secret value: {secret.Value}");
    }
}
