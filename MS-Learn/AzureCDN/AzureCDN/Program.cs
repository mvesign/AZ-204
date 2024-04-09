using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.Cdn.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCDN;

public static class Program
{
    private const string AccessToken = "[ACCESS-TOKEN]";
    private const string SubscriptionId = "[SUBSCRIPTION-ID]";
    private const string ResourceGroupName = "[RESOURCE-GROUP-NAME]";
    private const string CdnProfileName = "[CDN-PROFILE-NAME]";
    private const string CdnEndpointName = "[CDN-ENDPOINT-NAME]";

    private static void Main(string[] args)
    {
        var cdnClient = new CdnManagementClient(
            new TokenCredentials(AccessToken)
        )
        {
            SubscriptionId = SubscriptionId
        };

        if (!cdnClient.CheckProfileAndEndpointExist())
        {
            _ = cdnClient.CreateCdnProfile();
            _ = cdnClient.CreateCdnEndpoint();
            cdnClient.PurgeCdnEndpoint();
        }
    }

    private static bool CheckProfileAndEndpointExist(this CdnManagementClient cdnClient)
    {
        // List all the CDN profiles in this resource group
        var profiles = cdnClient.Profiles.ListByResourceGroup(ResourceGroupName);
        var profile = profiles.FirstOrDefault(p =>
            string.Equals(p.Name, CdnProfileName, StringComparison.OrdinalIgnoreCase)
        );
        if (profile == null)
        {
            Console.WriteLine($"Profile name '{CdnProfileName}' doesn't exist");
            return false;
        }

        var endpointExist = false;
        Console.WriteLine("Endpoints:");
        var endpointList = cdnClient.Endpoints.ListByProfile(profile.Name, ResourceGroupName);
        foreach (var endpoint in endpointList)
        {
            Console.WriteLine("\t{0} ({1})", endpoint.Name, endpoint.HostName);
            if (string.Equals(endpoint.Name, CdnEndpointName, StringComparison.OrdinalIgnoreCase))
                endpointExist = true;
        }

        return endpointExist;
    }

    private static Profile CreateCdnProfile(this CdnManagementClient cdnClient)
    {
        Console.WriteLine("Creating profile {0}", CdnProfileName);
        var profile = new Profile
        {
            Location = "westeurope",
            Sku = new Sku(SkuName.StandardVerizon)
        };
        return cdnClient.Profiles.Create(ResourceGroupName, CdnProfileName, profile);
    }

    private static Endpoint CreateCdnEndpoint(this CdnManagementClient cdnClient)
    {
        Console.WriteLine("Creating endpoint {0} on profile {1}", CdnEndpointName, CdnProfileName);
        var endpoint = new Endpoint()
        {
            Origins = [ new("Contoso", "www.contoso.com") ],
            IsHttpAllowed = true,
            IsHttpsAllowed = true,
            Location = "westeurope"
        };
        return cdnClient.Endpoints.Create(ResourceGroupName, CdnProfileName, CdnEndpointName, endpoint);
    }

    private static void PurgeCdnEndpoint(this CdnManagementClient cdnClient)
    {
        Console.WriteLine("Purging endpoint. Please wait...");
        cdnClient.Endpoints.PurgeContent(
            ResourceGroupName, CdnProfileName, CdnEndpointName, new List<string>() { "/*" }
        );
        Console.WriteLine("Done.");
    }
}