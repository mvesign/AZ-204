# AZ-204 Certification

## (New) Terminology

- CAPEX
  
  - Captial Expenditure

- OPEX
  
  - Operational Expenditure

- ADD
  
  - Azure Active Directory

- RBAC
  
  - Role-based access control

- ARM
  
  - Azure Resource Manager

- HPC
  
  - High Performance Computing.

- FaaS
  
  - Functions as a Service.

- OLTP
  
  - Stands for the database type Online Transaction Processing.

- RUs
  
  - Stands for Request Units, which is the unit that will be charged when choosing a serverless platform as the capacity mode in CosmosDB.

- Fault domain
  
  - Logical groups of hardware within a data center that share the same power and network hardware.

- Update domain
  
  - Logical groups of hardware within a data center that might undergo maintenance or be rebooted at the same time.

- Proximity placement group
  
  - Logical grouping used to ensure resources are physically located close to each other. Usefull when low latency between resources are required.

- vCPU
  
  - Virtual Central Processing Unit. With a limit currently set to 20 vCPUs, meaning a limit of 20 virtual machines if each VM has the minimum of 1 vCPU set.

- Ephemeral
  
  - Term to describe a container. Where it states that a container should be able to start up and run for as long as needed, then can be stopped and destroyed untill needed again. Not being persistent. Any changes during run time will be gone after destroying the container.

## 01 Azure and Cloud fundamentals

#### Azure Resource Manager.

This manager is acting like the proxy between the requesting party (such as Azure Portal, SDKs or REST clients) and the Azure services (such as Azure App Service or Azure Storage).

During the process of incoming requests, it will authenticate and authorize the requesting party.

Also is it an idempotent orchestra of the desired Azure services. Which can be setup via Azure Resource Manager templates JSON files. Meaning when a resource already exists, a second request of the same JSON file and parameters doesn't affect the existing resource.

Plusside of the JSON files, is the Infrastructure-as-Code setup. These JSON files can than be used via Azure Powershell, Azure CLI or Azure Portal.

#### Questions

1. What is the difference between scaling vertically and horizontally?
   
   - *Vertically is adding more resources like CPU and memory to an application, also referred to as **scaling up**. Horizontally is running more instances of the application, each with their base CPU and memory, also referred to as **scaling out**.*

2. What is the resource provider for virtual machines?
   
   - *In ARM templates you need to use Microsoft.Compute/virtualMachines.*

3. With SaaS, you, as the consumer, manage data, access, and the operating system running the software. True or false?
   
   - *False. SaaS stands for Software as a Service. You, as a consumer, only gets access to certain sofware applications, which is maintained by the other party.*

4. Azure virtual machines are an example of which cloud service model?
   
   - *IaaS. Which stands for Infrastructure-as-a-Service.*

5. What is the maxinum number of resource groups an Azure resource can be a member of?
   
   - *This can be arrange with Tags. At the moment the limit is set to 50.*

## 02 Implementing IaaS Solutions

#### Azure Resource Manager (ARM) Tools extension

In Visual Studio Code we can install the extension ARM Tools, which helps us with the generations of ARM templates. By creating an empty JSON file in VS Code, and than type in `arm!`, a default ARM template will be generated.

And with the shortcut `CTRL` + `spacebar` within the same JSON file, helpfull suggestions of resource types, API versions and other important properties will be brought up.

#### Azure Container Registry (ACR)

Via the `docker` command it's possible to create containers locally. But at some point we want to deploy it in a cloud variant. For this we can use Azure Container Registry (ACR), where we can store our container images. But it can also store Helm charts here. ACR is the Azure solution for a similar thing like Nuget.

In order to create a container in the ACR, we need to have a Azure resource group and ACR.

```bash
az group create --name "mjoy-rg" --location "westeurope"
az acr create --resource-group "mjoy-rg" --name "mjoyacr" --sku Basic
```

You can see we are using the `Basic` sku. There are three variants; **Basic**, **Standard** and **Premium**.

Also noteworthy is that the name of a Azure Container Registry  must be in lowercase and can only contain alphanumeric and within a range of 5 and 50 characters. Otherwise an error will be displayed.

Before we can send any Docker images, we must be authenticated with our newly created Azure Container Registry. After that can we push Docker images.

```bash
az acr login --name "mjoyacr"
# Create an alias for our Docker image
docker tag demo:v3 mjoyacr.azurecr.io/demos/demo:v1.0
# Push the Docker image to our Azure Container Registry
docker push mjoyacr.azurecr.io/demos/demo:v1.0
# And list the pushed Docker image from our Azure Container Registry
az acr repository list --name mjoyacr -o tsv
```

Now we have three steps before our Docker image is available from the cloud. But this is simplified with **ACR tasks**.  The easiest one is **Quick tasks**, which will trigger the creation of a Docker image on the cloud itself with the help of our `Dockerfile`.

```bash
# Create the Docker image directly in our Azure Container Registry
az acr build --image demos/demo:v2.0 --registry "mjoyacr"
  --file Dockerfile .
# Download the image and create a container out of it locally
az acr run --registry "mjoyacr" --cmd '$Registry/demos/demo:v2.0'
  /dev/null
```

#### Azure Container Instances (ACI)

The Azure Container Instances can be used when we don't want to run Docker images locally, but on Azure. The Azure Container Instance is a similar concept of Kubernetes Pods setup. But here they call it **container groups**. The containers within a container group share the life cycle, resources, etc. They also share the same public IP address and port namespace, meaning port mapping isn't supported. It's also possible to have a single container within a container group.

First thing to enable Azure Container Instances is to enable the admin user.

```bash
# Enable the admin user
az acr update --name "mjoyacr" --admin-enabled true
# Get the password of the admin user
az acr credential show --name "mjoyacr" --query "passwords[0].value"
# Create a container instance
az container create --resource-group "mjoy-rg" --name "mjoydemo"
  --image "mjoyacr.azurecr.io/demos/demo:v1.0" --cpu 1 --memory 1
  --registry-login-server "mjoyacr.azurecr.io"
  --registry-username "mjoyacr"
  --registry-password "[ACR_PASSWORD]"
  --ports 80 --dns-name-label "mjoydemo"
# Check if the container is created
az container show --resource-group "mjoy-rg" --name "mjoydemo"
  --query "provisioningState"
# Get the FQDN of the created container
az container show --resource-group "mjoy-rg" --name "mjoydemo"
  --query "ipAddress.fqdn"
```

The output of the last command shows the **fully qualified domain name** which we can open in a browser. This will show the web application within the container.

#### Questions

1. What does the `Docker rmi my-image:latest` Docker CLI command do?
   
   - *This remove the latest version of the image with the name `my-image`.* 

2- If your solution needs two containers within the same container group, which host operating system should you use?

- *Multi-container groups currently only supports Linux containers.*

3- Which element can you add to an ARM template to define `apiVersion` for all resources of a specific type, so that you don't have to specify `apiVersion` for each resource?

- *There is no built-in mechanism in ARM templates to define a default `apiVersion` for all resources of a specific type, as each resource is treated as a separate entity with its own properties and configurations*

4- We know we can run Linux containers on a Windows machine; Can we also run Windows containers on a Linux machine?

- *No. Although there are some workarounds to run Linux containers on a Windows machine (via WSL for example), the host must have a kernel that's compatible with the container.*

5- What is the minimum number of containers you can have within a container group?

- *A single container instance is technically its own container group.*

## 03 Creating Azure App Service Web Apps

Azure App Service  can host (web) applications and automated business processes with **WebJobs**. It can also host containers, or mulit-containers, with **Docker Compose**. And it has the option to setup an isolated environment, called **App Service Environment** (ASEs). Next to this, it's also complaint on ISO, PCI (Payment Card Industry) and SOC (System and Organization Control).

Possible to have multiple App Service plans, with multiple App Service applications per plan. Mainly because Windows and Linux apps can't run on the same plan, and an application isn't bound to use all of the plans resources. Each plan defines the operating system, region, number of virtual machines (although it's considerd a PaaS solution, it still runs on virtual machines but without our concern), size of those virtual machines and the pricing tier.

Several tiers are predefined. *Free* and *Shared* are for testing and development scenarios. Also will these tiers use the same virtual machines as other App Service applications, inclusing other customer's apps. And have these tiers allocated resource quota's, meaning scaling out isn't possible.

Possible to create an App Service plan via the Azure Portal, but also via the CLI. In the command below I've choosen for the SKU `B1`, which stands for `Basis`.

```bash
az appservice plan create --name "mjoy-asp-linux"
  --resource-group "mjoy-rg" --sku "B1" --is-linux
```

Also possible to list the runtimes that is by default supported for a **App Service web app**, via the following CLI command.

```bash
az webapp list-runtimes --os-type linux -o tsv
```

Based on this information, we can create a App Service web app that is linked to our created App Service plan. This can be done via the Azure Portal, but also via CLI.

```bash
az webapp create --name "mjoy-wa-windows" --resource-group "mjoy-rg"
  --plan "mjoy-asp"
# Go to the predefined website directory.
cd C:\Projects\_Courses\AZ-204\Chapter03\02-BasicHtmlWebApp
# Deploy the predefined website as a static HTML app
az webapp up --name "mjoy-wa-windows" --html --launch-browser 
```

But this is just a static HTML application. What about a containerized application. For this we need to recreate an Azure Container Registry. And create a Azure Service web app based on the created container.

```bash
# First go to the predefined directory.
cd C:\Projects\_Courses\AZ-204\Chapter03\03-AppServiceContainer\AZ204
# And create a new ACR
az acr create --resource-group "mjoy-rg" --name "mjoyacr"
  --sku "Basic"
# Enable admin privileges
az acr update --name "mjoyacr" --admin-enabled true
# Get the admin password for the next step
az acr credential show --name "mjoyacr" --query "passwords[0].value"
# Create container images version 1.0.0 and latest
az acr build --image "chapter03:1.0.0" --image "chapter03:latest"
  --registry "mjoyacr" --file "Dockerfile" .
# Update Linux App Service web app with container
az webapp config container set --resource-group "mjoy-rg"
  --name "mjoy-wa-linux"
  --docker-registry-server-url "https://mjoyacr.azurecr.io"
  --docker-custom-image-name "mjoyacr.azurecr.io/chapter03:latest"
  --docker-registry-server-user "mjoyacr"
  --docker-registry-server-password "[ACR_PASSWORD]"
```

#### Authentication

Now we have a webapp that can be used by all users of the world wide web. But what about authentication and authorization? Within App Service we can enable a module that will handle the following:

- Authenticate users with the (external) identity provider.

- Validates, stores and refreshed tokens.

- Manages authentication sessions.

- Injects identity information into request headers.

For Linux and container apps the module runs in a separate container, isolated from our code. This makes it framework independent, providing relevant information through the request headers.

When using provider's SDK, the sign-in process is referred to as **client flow**. Without the provider's SDK, it's referred to as **server flow**. 

#### Application settings

Each latest .NET application is working with `appsettings.json` files to store its application settings. These settings can be overriden with sensitive variant of a setting via the blade **Environment variables** of an App Service web app. Via the Azure Portal we can get an overview of all application setting overwrites in JSON format via the **Advanced edit** action.

Next to it being possible to overwrite application settings via Azure Portal, it can be editted via CLI.

```bash
az webapp config appsettings set --resource-group "mjoy-rg"
  --name "mjoy-wa-linux" --settings "MJOY_CUSTOM_VALUE=Different value"
```

#### CORS

CORS is, by default, a topic in web application development. In the App Service web app we need to specify which origins are allowed to reach our API. This can be done via the Azure Portal's **CORS** blade. But also via CLI.

```bash
az webapp cors add --resource-group "mjoy-rg" --name "mjoy-wa-linux"
  --allowed-origins "https://some.actual.domain"
```

#### Logging

Logging can be enabled per App Service. **Application logging** and **Deployment Logging** are avaiable for Windows and Linux based App Services. But for Windows based App Services can have several logging types more.

- Detailed error logging.
  
  - `.htm` error pages are stored in the App Service filesystem when an HTTP error code of `400` or greater occurs.

- Failed request tracing.
  
  - Detailed tracing information of failed requests, including traces of the IIS components, are stored in the App Service filesystem.

- Web server logging.
  
  - Raw HTTP requests data, stored in W3C extended log file format, are stored in either App System filesystem or Azure Storage blobs.

Logging can be enabled easily via the Azure Portal in the App Service **App Service logs** blade.

#### Questions

1. Can you have separate auto scale settings for each App Service within an App Service plan?
   
   - *No. The App Service plan is the component in charge of the resources.*

2- With authentication enabled and not using the provider SDK, once a user has authenticated with the identity provider, which URL does the provider redirect the client to?

- *It redirects the user to the setup **general available** (GA) endpoint; `/.auth/login/<provider/`*

3- Application logging can be enabled for both Windows and Linux App Service apps. True or false?

- *True. Together with **Deployment logging**, **Application logging** can be enabled*.

4- The private endpoint feature of App Service can be used to prevent access to your application from outside of your specified VNet. True or false?

- *True. This feature uses a private IP address from your VNet, which regulates that only inbound traffic can come from within your VNet.*

5- Which networking feature can be configured to allow your appliciation to make outbound calls to resources within your on-premise network?

- *VNet integration.*

## 04 Implementing Azure Functions

#### Exploring Azure Functions

Azure Functions provide a serverless platform on which to run blocks of code, also known as **functions**, that responds to events. The unit of deployment is called a **function app**. One function app can contain mulitple functions, but scaling is done per function app. So it is wise to group functions that share similar logic into one function app.

Next to Azure Functions, a similar solution is **Logic Apps**. With Logic Apps being more **declarative**, with a *designer-first* focus. Whereas Azure Functions is more **imperative**, with a *code-first* focus. Also does Azure Functions can be monitored via **Application Insights**. Whereas Logic Apps can be monitored via Azure Monitor logs.

Next to Azure Functions, a similar solution is **WebJobs**. Both build with the WebJobs SDK, build on App Service. But Azure Functions;

- Uses a serverless application model with automatic scaling without additional configurations.

- Has the ability to be developed and tested within the browser.

- Can be triggered on HTTP and Azure Event Grid events.

- Uses a **pay-per-use** pricing model.

#### Azure Functions hosting options

- **Consumption** is the default hosting plan for function apps, also referred to as **Serverless**. This plan will scale a function app based on the incoming requests. And is being billed only for the number of executions, execution time and used memory, also know as **GB-seconds**.
  
  After a period of idle time the function app can be scaled to zero, which can cause latency during a *cold startup* when scaling up from zero.
  
  It has a grant of 1,000,000 executions and 400,00 GB-seconds each month.

- **Premium** is similar to Consumption hosting plan, but it uses pre-warmed workers. Meaning no latency after being idle. Intended for function apps that needs to be run (nearly) continously, longer than the execution time limits of Consumption, or run on a custom Linux image.
  
  It uses **Elastic Premium (EP)** App Service plans, requiring an existing App Service plan using one of the Elastic Premium SKUs. Difference with this service plan is the **Elastic Scale out** configuration with the number of always-ready instances.
  
  Billing is based on the number of core seconds and memory allocation across all instances. No execution charge, but a minimum charge each month regardless if any function has been running.

- **App Service plan** like the same in previous chapter, also referred to as **Dedicated**. Which can be useful when having underutilized App Service plan resources or we want to provide a custom image for our functions.

Regardless of the hosting plan, every function app requires an Azure storage account that supports queues and tables for the functions code files, as well as operations such as managing triggers and logging executions. HTTP triggers don't require storage.

#### Developing, testing and deploying Azure Functions

A function app can contain multiple functions. But a function app starts with a `wwwroot` containing a `host.json` file. Each function within this function app, will have its own directory, containing the function script files. One of them being the `function.json` file that contains the configuration of the function.

Within this file, we can configure the type of trigger being setup for that function. The common trigger types that can be used within a function can be;

- `queueTrigger`
  
  - Stands for the data operation trigger, where the data is retrieved from a queue.

- `timerTrigger`
  
  - Stands for a time based trigger, which will be called between a predefined period in **crontab** format.

- `httpTrigger`
  
  - Stands for a webhook, which can be called via an URI. But it requires `code` parameter in order to be triggerd by Azure. If this code isn't present, a **401 HTTP status code** will be returned instead.

#### Durable functions

Durable Functions is an extension of Azure Functions which we can use to write stateless and serverless workflows, or also called **orchestrations**. These stateful workflows can be defined with **orchestrator functions** with the stateful entities as **entity functions**. Four types of durable functions;

- Client functions, or also know as *starter functions*.
  
  - Can use all function triggers (e.g. queueTrigger). But is required to initiate an orchestration workflow.

- Orchestrator functions
  
  - Defines the steps within the workflow. Doesn't perform any activities, just for orchestration. But it can handle all occurred errors within the workflow.

- Activity functions
  
  - Implement the steps within the workflow. Can make use of all input and output bindings available to functions.

- Entity functions, or also know as *durable entities*.
  
  - Defines the operations to manage small piece of state, invoked from *client* or *orchestrator* functions, accessed via unique entity identifier.

###### Durable functions patterns

- Function chaining
  
  - For when a specific order of functions needs to be followed. With the output binding of one given as an input binding of another. Where an archestrator function keeps track of the progress.

- Fan-out/fan-in
  
  - For multiple functions executed in parallel. And progress waiting for all to be completed.

- Asynchronous HTTP APIs
  
  - When coordination between long-running operations and external clients is required. Orchestrator function manages polling the status until completion or a time-out.

- Monitor
  
  - For recurring processes within a workflow that needs to be polled until conditions are met. Orchestrator function calls an activity function that checks the conditions being met.

- Human interaction
  
  - When an external approval is required. Orchestrator function uses a *durable timer* and waits for an external event before moving to the next function. Possible to setup a time-out with a remediation or escalation action.

- Aggregator
  
  - When data over a period of time needs to be aggregated into a single durable entity. Where data can be from multiple sources at varying volumes. 

#### Questions

1. What is the name of the file that holds the information on a function's triggers and bindings?
   
   - *That would be the `functions.json` file.*

2. What information is contained in the `AzureWebJobsStorage` application setting?
   
   - *It contains the connection string for an Azure Storage account that the Functions runtime uses for operations.*

3. Which durable function type does an orchestrator function call to implement the steps of a workflow?
   
   - *It calls an activity function.*

4. Which file is used to define application settings that only apply to local development?
   
   - *It uses the `local.development.json` file.*

## 05 Developing solutions that use Cosmos DB storage

#### Azure Table Storage

Azure Table Storage is a PaaS solution suited for storing non-relation data in the cloud. Access can be rapid and cost-effective, making it less expensive than standard SQL or the Cosmos DB service. It is part of the Azure Storage platform and has almost no limitations, except:

- Maximum size of a single entity is 1 MB.

- Number of properties per entity between 3 and 255.

- Requires provisioning of *general-purpose* storage account with **Standard** performance tier.

- It must be deployed with the DNS queue name in the data center of choice.

- The account name and name of the tables must start with a letter and between 3 and 63 characters long.

To access data within Azure Table Storage, it is recommended to use **Azure Storage Explorer**. It has the following components

- Account
  
  - Provides HTTPS-enabled endpoints of connection for RESTful requests.

- Table
  
  - Collection of logical grouped entities (e.g. customers, orders).

- Entity
  
  - Collection of schema-free properties, equivalent to a data row.

- Property
  
  - Meaningful combination of key-value pairs for storing NoSQL data.

Required to have 3 properties within an entity. The unique combination of `PartitionKey` and `RowKey`, to be able to retrieve an entity. The third property contains a timestamp value of last modification. Each property has a data type, so it can be selected based on its values and data type. Possible data types can be:

```plaintext
string, int32, int64, decimal, guid, boolean and binary
```

If we want to create a Azure Table Storage, we need to create a resource group and storage account. After that can we create a table with some entities and generate a **Shared Access Signature** in order to perform RESTful requests on the entities.

```bash
az group create --name "mjoy-rg" --location "westeurope"
az storage account create --name "mjoysa" --resource-group "mjoy-rg"
# Get the key of the storage account
az storage account keys list --account-name "mjoysa" --query [0].value
# Or get the entire connection string when using the SDK
az storage account show-connection-string --name "mjoysa"
  --resource-group "mjoy-rg"
# Create a new table
az storage table create --name "customers" --account-name "mjoysa"
  --account-key "[ACCOUNT_KEY]"
# Fill the table with an entity
az storage entity insert --account-name "mjoysa"
  --account-key "[ACCOUNT_KEY]"
  --entity PartitionKey=ReSellers RowKey=Contoso IsActive=true IsActive@odata.type=Edm.Boolean
  --if-exists fail --table-name customers
# Create SAS for the new table
az storage table generate-sas --name "customers" --account-name "mjoysa"
  --account-key "[ACCOUNT_KEY]"
  --permissions r
  --expiry 2200-01-01
```

After these commands we have an Azure Table Storage with an entity and a SAS. The table can be viewed in Azure Portal under the **Tables** blade of the storage account. To get the stored data, we can perform the following HTTP request.

```bash
https://mjoysa.table.core.windows.net/customers()
  ?[SAS]&$format=json
```

With the first parameter being the SAS value we retrieved with the `az storage account keys list` command. And the second the output format as JSON. It is possible to filter and search.

```bash
# Filter "IsActive = true"
https://mjoysa.table.core.windows.net/customers()
  ?[SAS]&$format=json
  &$filter=IsActive%20eq%20true
# Query entity based on PartitionKey and RowKey
# Note the case sensitivity on the key and value of the property.
https://mjoysa.table.core.windows.net/customers(
    PartitionKey='ReSellers',RowKey='Contoso'
  )
  ?[SAS]&$format=json
```

#### Azure Cosmos DB

To make use of Azure Cosmos DB, there are several APIs available.

- Table API
  
  - Similar to *Azure Table Storage* but with better performance. When using the Azure Table Storage, it requires no code changes when migrating to *Table API*. The Table API only work with **Online Transaction Processing** (OLTP) scenarios.

- MongoDB API
  
  - API compatible with *MongoDB Wire Protocol*. Uses BSON format to store data. CosmosDB does not utilize native MongoDB code. It is compatible with MongoDB server version 4.0, 3.6 and 3.2. Applications that uses MongoDB, can switch easily to CosmosDB by updating the connectionstring.

- Cassandra API
  
  - Guarantees backwards compatibility with *Apache Cassandra* products. Database to store large volumes of data in a column-oriented design. Can work with CosmosDB and still use native Apache Cassandra capabilities; Meaning we can use **Cassandra Query Language** (CQL) through the CQL shell while connected with CosmosDB. Also only works with **Online Transaction Processing** (OLTP) scenarios. 

- Gremlin API
  
  - Able to store data, query edges and vertices. Best for scenarios with complicated relationships. Supports the open source Gremlin wire protocol; Meaning applications can use open source Gremlin SDKs. Gremlin API uses same **Graph Query Language** as Apache Tinkerpop. And also only works with **Online Transaction Processing** (OLTP) scenarios.

- Core (SQL) API
  
  - *Native solution in Azure*, maintaining data in document format; Explaining why the CosmosDB services where previously named DocumentDB. Query language based on T-SQL, making it the ideal option when migrating from relational databases. It provides seperation between analytic and operational workload.

We must choose, when provisioning a CosmosDB account, between the capacity modes **provisioned throughput** or **serverless platforms**. Provisioned throughput charges a fixed amount per month, while serverless platforms charges on consumption, also called **Request Units** (RUs). After making the choices of capacity mode or database API, changes aren't possible.

Below the commands to setup a database with one container (similar to a table).

```bash
# First create the resource group
az group create --name "mjoy-rg" --location "westeurope"
# Then create the CosmosDB account
az cosmosdb create --name "mjoy-cosmosdb" --resource-group "mjoy-rg"
# And create the CosmosDB database with Core SQL API
az cosmosdb sql database create --name "mjoycosmosdb"
  --resource-group "mjoy-rg" --account-name "mjoy-cosmosdb"
# Followed by creating the CosmosDB container
az cosmosdb sql container create --name "CloudShops"
  --resource-group "mjoy-rg" --account-name "mjoy-cosmosdb"
  --database-name "mjoycosmosdb" --throughput "400"
  --partition-key-path "/OrderAddress/City"
```

###### Indexing

JSON documents are converted to a tree representation, with each property as a node within the tree. Querying will use the **Index Seek** algorithm in order to use the proper index, which can reduce **Resource Units** consumptions.

You can exclude a node from the indexing process, but this will exclude the parent node as well. A node is references by a path (e.g. `/Customer/Name/?`  or `/Customer/*`). Where the `?` character includes only the exact value of the node. And the `*` character includes all nested nodes.

Custom indexes are possible, but must be of the following types.

- Range index
  
  - Suite a single field containing a list of *string* or *number* fields. Useful with filter, orden and join request. Best to use for any single field.

- Composite index
  
  - Similar to **range index**, but best to use when performed on multiple fields.

- Spatial index
  
  - Uses geospatial objects.

###### User-defined functions

A **user-defined function** (UDF) is a piece of predefined logic that can be used within a query. Language to create a user-defined function is **Javascript**. It can have multiple parameters and one result value. It should be avoided when:

- It is a replication of existing CosmosDB functionality.

- The query only has a user-defined function in its `WHERE`  clause.

###### Triggers

Within CosmosDB we have two types of triggers, **pre-triggers** and **post-triggers**. Difference with triggers from relational databases, is that CosmosDB triggers should be called explicitly from the SDK. Similar to *user-defined functions* and *stored procedures*, a trigger is created in **Javascript**. To create a trigger, it's best to do so in Azure Portal via the **Data Explorer** tab of the Azure CosmosDB account.

An example of a trigger

```javascript
function validateOrder() {
    var theContext = getContext();
    var theRequest = theContext.getRequest();
    var item = theRequest.getBody();
    if (item["OrderCustomer"] != undefined && item["OrderCustomer"] != null) {
        theRequest.setBody(item);
    }
    else {
        throw new Error('OrderCustomer must be specified');
    }
}
```

###### CosmosDB REST API

Clients can manage documents or database collections programmatically via the CosmosDB REST API. When using this API an authorization header is required, containing a token generated via the CosmosDB REST API.

When multiple clients are managing documents or database collections, the use of an additional value is required. This value is part of the **Optimistic Concurrency Control** (OCC), and comes in the property `_etag`. This `_etag` can be managed via the CosmosDB SDK programmatically.

```csharp
var accessCondition = new AccessCondition
{
    Condition = readDoc.ETag,
    Type = AccessConditionType.IfMatch
};
await client.ReplaceDocumentAsync(
    readDoc,
    new RequestOptions
    {
        AccessCondition = accessCondition
    }
);
```

#### Questions

1. Which APIs are supported by CosmosDB?
   
   - *There are five at the moment. Table API, MongoDB API, Cassandra API, Gremlin API and the default Core (SQL) API.*

2- What is the difference between a composite index and a range index?
   
   - *Range index consist on a single field of the string or number type, while composite consist of multiple fields.*

3- What is the defailt CosmosDB backup option?
   
   - *Periodic backup. Where the retention policy is limited by month, with backup intervals at a minimum of 1 hour.
     Also noteworthy is that a restoration required sending a request to the support team.*

4- Can you execute the CosmosDB trigger from the Azure portal?
   
   - *No. They should explicitly be called from the CosmosDB SDK.*

5- What language is used for stored procedures?
   
   - *Next to triggers, stored procedures are written in Javascript.*

## 06 Developing solutions that uses Azure Blob Storage
