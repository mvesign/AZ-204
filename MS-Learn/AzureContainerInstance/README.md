## Setup

Need to perform the following commands before this application can be executed.

```bash
# In order to login with your own credentials at Azure via a terminal.
# This command returns a code with a URL.
#  - Copy the code.
#  - Follow the URL.
#  - Insert the previously copied code.
az login --use-device-code

# Create a new resource group where we can create a new storage account under.
az group create --name "mjoy-rg" --location "westeurope"

# Create a Azure Container Instances service within the resource group.
az container create --resource-group "mjoy-rg" --name "mjoyaci"
  --image mcr.microsoft.com/azuredocs/aci-helloworld --ports 80
  --dns-name-label "mjoy-aci-dns-01" --location "westeurope"
```

After these commands we have setup a Azure Container Instances account.

### Specify restart policy

By default the `Always` restart policy is used, but this can be overwritten by using the `--restart-policy` parameter when performing the `az container create` command. Chooses we have are `Never` or `OnFailure`.

```bash
az container create --resource-group "mjoy-rg" --name "mjoyaci"
  --image mcr.microsoft.com/azuredocs/aci-helloworld --ports 80
  --dns-name-label "mjoy-aci-dns-02" --location "westeurope"
  --restart-policy "OnFailure"
```

### Specify environment variables

Possible to give environment variables by using the `--environment-variables` parameter when performing the `az container create` command. For example.

```bash
az container create --resource-group "mjoy-rg" --name "mjoyaci"
  --image mcr.microsoft.com/azuredocs/aci-helloworld --ports 80
  --dns-name-label "mjoy-aci-dns-03" --location "westeurope"
  --environment-variables 'NumWords'='5' 'MinLength'='8'
```

### Mount Azure file share

Can mount a file share to the Azure Container Instances service. Benefit is that the state of the container can be stored within this file and be persisted when the container is terminated after its execution. It does require the setup of an Azure storage account.

```bash
az container create --resource-group "mjoy-rg" --name "mjoyaci"
  --image mcr.microsoft.com/azuredocs/aci-helloworld --ports 80
  --dns-name-label "mjoy-aci-dns-04" --location "westeurope"
  --azure-file-volume-account-name $ACI_PERS_STORAGE_ACCOUNT_NAME
  --azure-file-volume-account-key $STORAGE_KEY
  --azure-file-volume-share-name "Logs"
  --azure-file-volume-mount-path /aci/logs/
```

## Verify

Now that the Azure Container Instances service is created, we can verify the state of it.

```bash
# Show all status details of the created Azure Container Instances.
az container show --resource-group "mjoy-rg" --name "mjoyaci"
# Show all status details of the created Azure Container Instances in a neatly table format.
az container show --resource-group "mjoy-rg" --name "mjoyaci" --output table
# Can also display specific status details with the "--query" parameter.
az container show --resource-group "mjoy-rg" --name "mjoyaci"
  --query "{FQDN:ipAddress.fqdn,ProvisioningState:provisioningState}"
  --output table
```

In the last command we retrieve the FQDN (fully qualified domain name), which we can open in a browser to see it running.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```
