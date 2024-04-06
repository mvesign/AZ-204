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

# Create a Azure Cosmos DB account within the resource group
# Make sure to take note of the resulting 'documentEndpoint' value.
az cosmosdb create --name "mjoy-cosmos-acc" --resource-group "mjoy-rg"

# Get the primary key required to connect with our Azure Cosmos Db instance.
# Explicitly we require the 'primaryMasterKey' value.
az cosmosdb keys list --name "mjoy-cosmos-acc" --resource-group "mjoy-rg"
```

After these commands we have setup a Azure Cosmos DB account, and can take notes of the required credentials.

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```

## Stored procedures

Next to progmatically managing Azure Cosmos DB, it can make use of stored procedures which are required to be setup beforehand.

```javascript
var createDocumentStoredProc = {
    id: "createPerson",
    body: function createPerson(personToCreate) {
        var context = getContext();
        var collection = context.getCollection();
        var accepted = collection.createDocument(collection.getSelfLink(),
              personToCreate,
              function (err, personToCreate) {
                  if (err) throw new Error('Error' + err.message);
                  context.getResponse().setBody(personToCreate.id)
              });
        if (!accepted) return;
    }
}
```