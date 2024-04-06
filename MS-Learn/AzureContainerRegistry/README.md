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

# Create a Azure Container Registry within the resource group
az acr create --name "mjoyacr" --resource-group "mjoy-rg" --sku Basic
```

After these commands we have setup a Azure Container Registry.

## Create image and show results

In order to create a new image into the Azure Container Registery, with the local `Dockerfile`, run the following command.

```bash
az acr build --image sample/hello-world:v1 --registry "mjoyacr" --file Dockerfile .
```

Now that the command is performed, we can check if the image is actually present with the following commands.

```bash
# Show the images present in our repository.
az acr repository list --name "mjoyacr" --output "table"

# And show the tags of our image.
az acr repository show-tags --name "mjoyacr" --repository "sample/hello-world" --output "table"
```

## Run image via ACR

Now that we know the image is created, we can run it.

```bash
az acr run --registry "mjoyacr" --cmd '$Registry/sample/hello-world:v1' /dev/null
```

Make note that we use special parameter `$Registry` to specify the registry the command is being run from.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```
