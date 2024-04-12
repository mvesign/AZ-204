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

# Create a Azure Service Bus namespace.
az servicebus namespace create --name "mjoy-sbns" --resource-group "mjoy-rg" --location "westeurope"

# Within the Azure Service Bus namespace create a Azure Service Bus queue.
az servicebus queue create --name "mjoy-sbq" --namespace "mjoy-sbns" --resource-group "mjoy-rg"
```

After these commands we have setup a Azure Service Bus namespace and queue.

Next step is to gather the connectionstring from the Azure Portal. This can be done by.

1. Going to the Azure Portal and select the created Azure Service Bus namespace.
2. On the left side, under `Settings`, click on `Shared access policies`.
3. On the right side click on the policy `RootManageSharedAccessKey`.

This will reveal the keys and connectionstrings available via root management. Copy one of the two connectionstring values. And replace the `ServiceBusQueueConnectionString` class variable within the `Program.cs` file.

## Execute the program

You can execute the program by running the command `dotnet run`. But make sure that the proper connectionstring is setup.

When executing, the following text will be written in the console. Where `{x}` is equal to the number of created messages.

```text
A batch of {x} messages has been published to the queue.
Press any key to continue
```

Wait with pressing any key to continue the program. Instead.

1. Go back to the Azure Portal and select the created Azure Service Bus namespace.
2. On the left side, under `Entities`, click on `Queues`.
3. On the right side click on the created Azure Service Bus queue entity.
4. On the left side click on `Service Bus Explorer`.
5. On the right side click on `Peek from start` button.

This will display the created messages currently present within the queue.

Now we can go back to our console and pres any key in order to continue the program, displaying the following new lines in the console.

```text
Wait for a minute and then press any key to end the processing
Received: Message 0
Received: Message 1
Received: Message 2
```

If we check back in the `Service Bus Explorer` and click on the `Peek from start` button again, we will see no messages being present in the queue.

Back in the console click any key to continue the program again, which will stop further processing of messages and finalize the program as a whole. Also does the console output this with the following new lines.

```text
Stopping the receiver...
Stopped receiving messages
```

## Clean up

After we are done, we don't want the resources to be hanging in Azure. So, let's clean them up by deleting the entire resource group.

```bash
# Add the '--no-wait' flag to not wait in CLI, for the resources being cleaned up.
az group delete --name "mjoy-rg" --no-wait
```
