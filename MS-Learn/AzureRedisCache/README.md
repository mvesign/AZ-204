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

# Create a Azure Redis Cache instance within the resource group.
az redis create --name "mjoy-redis" --resource-group "mjoy-rg" --location "westeurope" --sku Basic --vm-size c0

# Next we need to get the access keys, so we can construct our connectionstring.
az redis list-keys --name "mjoy-redis" --resource-group "mjoy-rg"
```

After these commands we have setup a Azure Redis Cache instance.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```
