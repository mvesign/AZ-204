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

# Create a storage account within the resource group
az storage account create --name "mjoyblobsa" --resource-group "mjoy-rg" --location "westeurope"
```

After these commands we have setup a storage account, for which we need to get the credentials required for our application.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```