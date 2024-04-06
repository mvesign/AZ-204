## Setup

Need to perform the following commands before this excercise can be performed.

```bash
# In order to login with your own credentials at Azure via a terminal.
# This command returns a code with a URL.
#  - Copy the code.
#  - Follow the URL.
#  - Insert the previously copied code.
az login --use-device-code

# Install the Azure Container Apps extension for the CLI.
az extension add --name containerapp --upgrade

# Register required namespaces to be available.
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights

# Create a new resource group.
az group create --name "mjoy-rg" --location "westeurope"
```

## Create an environment

Need to create an environment to group container apps. Container apps within the environment, will write to the same Log Analytics workspace. So performing this command will also setup a Log Analytics workspace, because we aren't providing an existing one.

```bash
az containerapp env create --name "mjoy-cae" --resource-group "mjoy-rg" --location "westeurope"
```

## Create a Azure Container App

Next we can deploy a Azure Container App into a environment. We will also set `--ingress` to `external` so that it will be externally accessible.

```bash
az containerapp create --name "mjoy-ca" --resource-group "mjoy-rg" --environment "mjoy-cae"
  --image mcr.microsoft.com/azuredocs/containerapps-helloworld:latest --target-port 80
  --ingress 'external' --query properties.configuration.ingress.fqdn
```

With the `--query` parameter we return the FQDN (fully qualified domain name) which we can use to access the Azure Container App.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```
