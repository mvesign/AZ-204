# AZ-204 Certification

## (New) Terminology

- CAPEX
  
  - Captial Expenditure

- OPEX
  
  - Operational Expenditure

- ADD
  
  - Azure Active Directory

- OIDC
  
  - OpenID Connect

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
     2-  What is the difference between a composite index and a range index?
   - *Range index consist on a single field of the string or number type, while composite consist of multiple fields.*

3- What is the defailt CosmosDB backup option?

- *Periodic backup. Where the retention policy is limited by month, with backup intervals at a minimum of 1 hour.
  Also noteworthy is that a restoration required sending a request to the support team.*

4- Can you execute the CosmosDB trigger from the Azure portal?

- *No. They should explicitly be called from the CosmosDB SDK.*

5- What language is used for stored procedures?

- *Next to triggers, stored procedures are written in Javascript.*

## 06 Developing solutions that uses Azure Blob Storage

We can create Azure Storage Accounts and put files or **binary large objects** (blobs) inside these. A storage account is setup for a region, but can be synchronized over multiple regions and thus multiple storage accounts, to minimize the latency.

When creating an Azure Storage Account, it must be a DNS-unique name in lowercase letters and numbers (so no underscores or such). A container must be created for the Azure Storage Account, similar to a folder. Nesting of containers isn't possible, but virtual folders is. Blobs or files can than be stored in a container.

Two admin keys are created by default for managing purposes. No for application use. Applications should use **shared access signatures** (SAS) or access via **role-based access control** (RBAC).

Several types of access levels can be setup on a blob or container.

- Private access level
  
  - No anonymous accesss. Setup on container level. Requires an admin key to get access to blobs.

- Blob access level
  
  - Anonymous read access to the specific blob only.

- Container access level
  
  - Anonymous read access to all blobs within a container.

#### High availability and durability

Azure Storage Accounts have **Locally-Redundant Storage** (LRS), meaning it stores 3 copies of the data on different hardware. It also has **Zone-Redundant Storage** (ZRS), where copies are stored in different buildings in the same region, but with a separate source of power and internet.

It also has **Geo-Redundant Storage** (GRS) and **Geo-Zone-Redundant Storage** (GZRS), with the difference is that copies are stored over multiple regions.

And finaly it has **Read-Access Geo-Redundant Storage** (RA-GRS), if read access to the copy of the second region is desired.

#### Setting up blobs

When setting up a blob, we must select.

- The redundancy type from previous section.

- Performance tier. Which can be **Standard** or **Premium**.
  
  - Premium is not available for Azure Queue Storage or Table Storage.
  
  - Premium should be selected when performance is important.

- Access tier. Which can be **Hot**, **Cool** or **Archive**.
  
  - **Hot** being for data that requires a lot of read-write transactions.
  
  - **Cool** being for that can go for days without being accessed. But must persist for at least 30 days.
  
  - And **Archive** for rarely accessed data. But must persist for at least 180 days.

- Blob type. Which can be **Block**, **Append** or **Page**.
  
  - **Block** for composing blocks of data. Is the default type. Can include 50.000 different-size blocks up to a maximum of 400MB per block. Only type which allows *blob access level*.
  
  - **Append** for appending data to a blob, such as tracing and logging workloads. With a maximum size of 195GB.
  
  - **Page** for improving random access, such as storing virtual disks. Blob consists of 512-byte pages. When writing, it can overwrite just one page and commits wires immediately. With a maximum of 8TB.

The access tier can be configured with policy rules to move blobs around, before putting it up for deletion. This setup is very useful for logs and backup retention.

```bash
# First create the resource group
az group create --name "mjoy-rg" --location "westeurope"
# Than create the storage account
az storage account create --name "mjoysa" --resource-group "mjoy-rg"
# And get the first admin key and connection string
az storage account keys list --account-name "mjoysa" --query [0].value
az storage account show-connection-string --name "mjoysa"
  --resource-group "mjoy-rg"
# Create a container with the retrieved key
az storage container create --name "products" --public-access "blob"
  --account-name "mjoysa" --account-key "[STORAGE-ACCOUNT-KEY]"
# Upload a file to the created container
az storage blob upload --account-name "mjoysa"
  --container-name "products" --account-key "[STORAGE-ACCOUNT-KEY]"
  --file "logo.png" --name "logo.png"
# Then show the uploaded file
az storage blob show --name "logo.png" --account-name "mjoysa"
  --container-name "products" --account-key "[STORAGE-ACCOUNT-KEY]"
# This file is also accessible via its public URL
# https://mjoysa.blob.core.windows.net/products/logo.png
```

Below overview of setting up a statis website with an index and a 404-page.

```bash
# No need to create a new resource group.
# Than create the storage account
az storage account create --name "mjoysa2" --resource-group "mjoy-rg"
# And get the first admin key
az storage account keys list --account-name "mjoysa2" --query [0].value
# Enable the static website container
az storage blob service-properties update --account-name "mjoysa2"
  --static-website --404-document "404.html"
  --index-document "index.html" --account-key "[STORAGE-ACCOUNT-KEY]"
# Upload the static HTML pages
# Make sure the 404.html and index.html pages are locally present
az storage blob upload --account-name "mjoysa2"
  --container-name '$web' --account-key "[STORAGE-ACCOUNT-KEY]"
  --file "404.html" --name "404.html"
az storage blob upload --account-name "mjoysa2"
  --container-name '$web' --account-key "[STORAGE-ACCOUNT-KEY]"
  --file "index.html" --name "index.html"
# And finally get the public available URL
az storage account show --name "mjoysa2" --query "primaryEndpoints.web"
```

#### Encryption

The only and default option for encryption is **Azure Storage Service Encryption** (SSE). But it can be used with different kind of keys, such as Microsoft keys, keys hosted in Azure Key Vault or hosted by third-party services.

Files can be explicitly encrypted with available services such as **Azure Rights Management** (Azure RMS) or SDKs.

#### Metadata and tags

Searching for blobs can be simplified by setting up **blob index tags** as metadata of blobs. This metadata can be searched without downloading the actual content of the blob, by making use of **Azure Cognitive Service** via Web API requests.

When a blob is modified the index will be modified as well, to ensure that the blob is searchable.

#### Questions

1. How can you increase the availability of the Azure Blob Storage service?
   
   - *By default the data is stored in copies over 3 different hardware components. But you can choose over several redundancy types to increase the availability. Such as **Local-Redundant Storage** or **Geo-Redundant Storage**.*

2. What features are available to protect data from deletion?
   
   - *You have **point-in-time restore**, where you can restore one or more containers to an earlier state.
     You also have **soft delete** to recover blobs that were previously marked for deletion.
     And finaly you have **versioning**, to automatically maintain previous versions pf your blobs.*

3. Can you leverage an SDK to read blob metadata without downloading the blob?
   
   - *Yes. Needs to setup metadata in the container so an index can be build up.*

4. Can we migrate blobs between access tiers automatically?
   
   - *Yes. This can be done via setting up policy rules.*

5. What type of data can be persisted in blobs?
   
   - *Blob stands for **binary large objects**, such as files or text data.*

## 07 User Authentication and Authorization

Within Azure, Microsoft identity platform is used for authentication and authorization. This platform uses the standards **OAuth2.0** for authentication and **OpenID Connect** (OIDC). Where **OpenID Connect** is build on top of **OAuth2.0**, meaning it shares many  thermonology and flows.

#### Azure Active Directory

For authentication, setup of an **Azure Active Directory** (AAD) tentant is required for applications in order to delegate identity and access management functions to Azure Active Directory.

When setting up an application for an Azure Active Directory tenant, we determine if it is a single tenant app (also called the **home tenant** and meaning it can only access the Azure Active Directory it is registered at), or a multi-tenant app (meaning it can access other Azure Active Directory tenants as well).

During registration, an application object is created within the home tenant, visible in the **App registrations** blade of the Azure Active Directory. Which is a template for creating **service principals**. The setup of a service principal equals a representation of the application, and is located in the **Enterprise applications** blade of the Azure Active Directory.

Two types of permissions are available, **delegated permissions** and **application permissions**. Where a delegated permission is required when an application acts on behalf of a signed-in user. And application permission is required when the application acts on it's own behalf. Only admins can consent for the latter.

Three types of consents are available.

- **Incremental and dynamic user consent**
  
  - User can request minimal permissions upfront. When additional permissions are required, user will be prompted for consent of these permissions. Only available for delegated permissions.

- **Static user consent**
  
  - Specify all required permissions up front and requests consent to the user. Downside is all required permissions must be known in advance.

- **Admin consent**
  
  - When more privileged permissions is required. In case of highly privileged information, to keep control. When admin consent is setup, only admins will be prompted.

Final part is setting up conditional access. In regular scenarios of conditional access, no code changes are required. Setup can be done in the service principal of the **Microsoft Entra** portal.

#### Microsoft Authentication Library (MSAL)

Microsoft Authentication Library (MSAL) is the successor of Active Directory Authentication Library (ADAL), which was part of Azure Active Directory. Microsoft Authentication Library will do heavy lifting for us, such as.

- Usage of OAuth and OpenID Connect libraries.

- Handling of protocol level details.

- Obtain tokens on behalf of users or an application.

- Handle token expirations by caching and refreshing tokens.

- Supports any Microsoft identity.

- Deliver actionable exceptions, logging and telemetry.

#### Microsoft Graph

Microsoft Graph is to access data on **Microsoft 365 core services**. Three main components to help with access and flow of data.

- Microsoft Graph API

- Microsoft Graph connectors.
  
  - Used to bring external sources into Microsoft Graph. Such as Jira, SalesForce, etc.

- Microsoft Graph Data Connect

With the Microsoft Graph Explorer we can explor REST calls and see what data can be returned. The actual implementation is done via SDKs, such as the **core library** and the **service library**. Where the core library has the core functionalities. And the service library has models and builders for the Microsoft Graph metadata.

#### Shared Access Signatures

A shared access signature is a signed URI that provides defined access rights to specific resources within a storage account for a specific period.

Three types os shared access signatures available in Azure.

- User delegation SAS
  
  - Most secure because it is linked to Azure Acive Directory credentials. It is only supported for *Blob Storage*.

- Service SAS
  
  - Secured with the storage account key. Provides access to a resouce in either a Blob Storage, Table Storage and Azure Files.

- Account SAS
  
  - Secured with the storage account key. Provides access at the storage account level. Can access more than one service at the same time.

```bash
# First create a resource group and a storage account.
az group create --name "mjoy-rg" --location "westeurope"
az storage account create --name "mjoysta" --resource-group "mjoy-rg"
  --location "westeurope" --sku "Standard_LRS"
# Than list the automatically generated account keys.
az storage account keys list --account-name "mjoysta"
  --resource-group "mjoy-rg"
# Using one key to create a new storage container
az storage container create --name "mjoystc" --account-name "mjoysta"
  --account-key ""
```

In the storage accounts details, on the Azure Portal, we can create a shared access signature manually via the **Shared access signature** blade. Or we can create a new shared access signature via the created container and use a similar named blade. But via the container, we can create a **user delegation SAS**. Which will extend the shared access signature with additional query parameters.

###### Stored access policies

In Azure there is no option to keep track of generated shared access signatures. But we can setup a stored access policy and link new shared access signatures to it. With this policy we can setup the validity period and permissions, which will be inherited by the shared access signature.

#### Questions

1. Which service can be used to bring external data into Microsoft Graph applications and Services?
   
   - *Microsoft Graph connectors.*

2- Which types of permissions are required when an application needs to act on behalf of a signed-in user?
   
   - *Delegated permission.*

3- Which MSAL library supports single-page applications?
   
   - *MSAL.js, because it is being used in Javascript of Typescript web applications.*

4- Which type of client applications runs on user devices, IoT devices, and browsers?
   
   - *Public client applications.*

5- Which type of SAS is recommended where possible and only available with the Blob service?
   
   - *User delegation SAS.*

## 08 Implementing Secure Cloud Solutions

In **Standard** tier the keys, secrets and certificates are software-protected and safeguarded by Azure. **Premium** tier we can also import or generate keys in **hardware security modules** (HSMs).

#### Authorization

All of these configurations are stored in a **key vault**. A key vault is a logical group of configurations per application per environment (e.g. ConsoleApp for test). Secrets within a key vault are encrypted as rest. For key vaults we have two permission models.

- Role-based access control.
  
  - Manages access for creating and managing key vaults and their attributes and data via the *management plane*.

- Key Vault access policy.
  
  - Only for access of the data within a key vault via the *data plane*. Not managing the key vault.

We can setup a key vault via CLI.

```bash
az group create --name "mjoy-rg" --location "westeurope"
# Create a new key vault.
az keyvault create --name "mjoy-kv" --resource-group "mjoy-rg"
  --location "westeurope"
# Set an encrypted secret in the key vault.
az keyvault secret set --vault-name "mjoy-kv" --name "SecretKey"
  --value "SecretValue"
# Read the set secret, which will be decrypted automatically.
az keyvault secret show --vault-name "mjoy-kv" --name "SecretKey"
# Can also get a secret based on the version.
# Meaning secrets are versioned.
az keyvault secret show --vault-name "mjoy-kv" --name "SecretKey"
  --version "7ee11ec911fc4aae81679175d49a4c58"
```

#### Authentication

Possible to authenticate with *Key Vault* via **managed identities**. These managed identities will manage credentials itself, so storing and rotation are being handled. Managed identities are a special type of service principal only available with Azure resources. When deleting a managed identity, the corresponding service principal is also deleted. There are two types of managed identities.

The first one is **user-assigned managed identities**. This is a standalone resource tied to an Azure Active Directory identity. Can be assigned to one or more applications. But because it is standalone, the life cycle is managed separately.

```bash
# Create a new user-assigned managed identity.
az identity create --name "mjoy-uami" --resource-group "mjoy-rg"
```

The second one is **system-assigned managed identities** which is enabled within a resource and shares the life cylce of that resource.

```bash
# Create a new app service plan.
az appservice plan create --name "mjoy-asp" --resource-group "mjoy-rg"
  --sku "B1"
# Create a web app that uses the app service plan.
az webapp create --name "mjoy-wa" --resource-group "mjoy-rg"
  --plan "mjoy-asp"
# Now create a key value reference in Azure Portal manually
# And enable the system-assigned managed identity
az webapp identity assign --name "mjoy-wa" --resource-group "mjoy-rg"
```

#### Azure App Configuration

Application configuration can be stored centrally within Azure. Can add labels to a key-value pair to have duplicate keys, but with different labels to differentiat between them (e.g. Development or Production).

Can reference to a key within a Key Vault, to have the security of Key Vault. As well setting up a soft-delete, purge protection and a retention period before a soft-deleted key-value pair is actually deleted.

```bash
az group create --name "mjoy-rg" --location "westeurope"
# Create the App Config.
az appconfig create --name "mjoy-ac" --resource-group "mjoy-rg"
  --location "westeurope"
# And add a new key-value pair to it.
# The --yes parameter is added to skip confirmation for storing.
az appconfig kv set --name "mjoy-ac" --key "Chapter8:DemoApp:Greeting"
  --value "Hello, World!" --yes
# And add a similar key-value pairs, but with a label.
az appconfig kv set --name "mjoy-ac" --key "Chapter8:DemoApp:Greeting"
  --value "Hello, Development!" --label "Development" --yes
az appconfig kv set --name "mjoy-ac" --key "Chapter8:DemoApp:Greeting"
  --value "Hello, Development!" --label "Production" --yes
```

Also possible to setup feature flags, so that a specific feature will only be triggered when a flag matches the criteria. Configuring the feature flags can be done via the **Feature manager** blade of the Azure App Configuration resource.

#### Questions

1. Which type of managed identity shares its life cycle with an Azure resource?
   
   - *System-assigned managed identity.*

2. If you want to use App Configuration ro read a secret value, what is the recommended approach to achieve this?
   
   - *Store the secret value in a Key Value resource. Than in your App Configuration setup a Key Value reference.*

3. Which method of authenticating your app to Key Vault is recommended for most scenarios and doesn't require you to manage secret rotation?
   
   - *Via a managed identity.*

4. Which Key Vault permission model is used for controlling access to the vault's management plane?
   
   - *The role-based access contol, or RBAC.*

## 09 Caching and Content Delivery within solutions

Implement caching can be done via the so-called **cache-aside pattern**. Meaning there is a cache service next to your own applications, which should be faster than the regular database behind your applications. Two main possible scenarios.

- First is to setup the cache during the first request. Making the first request as the **cold request**, with the second and upcoming requests as the **hot request**. With the downside of the cold request can taking longer to finalize.

- Second is to setup a trigger to reload the cache before expiration. So no cold or hot requests are implied. But with the downside of extra memory usage, because the cached data is not being used immediatly.

#### Azure Cache for Redis

Redis stands for **Remote Dictionary Server** and goes back to 2009 in C as a memory management service. It uses the TCP protocol, default over the ports **6379** and **6380**.

In Azure, Redis cache is fully adopted as a **Azure Cache for Redis** resource, and available over several pricing tiers.

- Basic
  
  - No SLA provided, but ideal for development and testing. No clustering or geo-replication. And does not persist the caching data during restarts.

- Standard
  
  - Supports fail-over with primary/replica duplication. Still no clustering or geo replication. And does not persist the caching data during restarts. 

- Premium
  
  - Same as the *Standard* tier, but it provides clustering. Can persist the data during restarts. For this it has two synced nodes running, so it will hangover to the second node during a restart of the first node.

- Enterprise
  
  - Same as the *Premium* tier, but with extra features such as Redis Search, Redis Bloom, and active geo replication. Most suitable for business-critical applications.

- Enterprise Flash
  
  - Same as the *Enterprise* tier, but runs on fast non-volatile storage. Also supports Redis Flash and can have memory usage up to 1.5TB.

```bash
# Ofcourse we need a resource group first.
az group create --name "mjoy-rg" --location "westeurope"
# Setup redis instance with the Basic tier.
az redis create --name "mjoy-redis" --resource-group "mjoy-rg"
  --location "westeurope" --sku Basic --vm-size C0
# And check if it is successful by getting the key.
az redis list-keys --name "mjoy-redis" --resource-group "mjoy-rg"
  --query primaryKey -o tsv
```

The access key can also be accessed via the Azure Portal. Next to setting up firewall rules to allow access from specific IP or IP ranges. And managing virtual network integration, which is only available from the **Premium** tier and higher.

Another section is the **Diagnostic settings**, where we can configure the alerts for when the **memory consumption** or **connected clients** have reached the limits of the selected tier.

Redis cache supports string or binary data in a key-value structure. Best pratice is to use self-explanatory keys and values of less than 100KB. Bigger values are possible, but should be split up to enhance the performance. It supports the following data types within the values.

- Strings, with a maximum size of 512MB.
  
  - `GET`, `SET` or `GETSET` for managing string values.
  
  - `GETBIT` or `SETBIT` for managing binary values.
  
  - `INCR`, `DECR` or `INCRBY` for incrementing or decrementing integers stored in string values.
  
  - `APPEND` for appending to string values.
  
  - `GETRANGE` or `SETRANGE` for getting or updating a part of a string value.

- Lists, as a list of *strings*. Sorted by insertion order. Created by adding new elements to an empty key. Very fast when getting the start or end elements of the list.
  
  - `LPUSH` for adding elements to the start.
  
  - `RPUSH` for adding elements to the end.
  
  - `LRANGE` for getting inserted items.

- Sets, as an unsorted list of *strings*. Can only contain unique elements. Adding same element will overwrite the existing element.
  
  - `SADD` for adding new string values.
  
  - `SCARD` for getting the number of elements.
  
  - `SMEMBER` for getting the actual elements.

- Hashes, as collections of mapped string field and string values. Mainly for storing objects.
  
  - `HMSET` for adding fields to a hash.
  
  - `HGETALL` for listing all fields in a hash.

In the main overview of the **Azure Cache for Redis** resource in the Azure Portal we can connect with the Redis Console. Here we can execute the following commands.

```bash
# Set the string key "client" with a value.
SET client TheCloudShops
GET client
# Append the string value to existing string key "client".
APPEND client ' best reseller'
GET client

# Set the string key "count" and increment it.
SET count 1
INCR count
GET count

# Set a list of three elements and show all elements.
LPUSH orders 19
LPUSH orders 23-R
RPUSH orders 77
LRANGE order 0 -1
# Pop one element from the list.
LPOP orders
LRANGE order 0 -1

# Set a set with two elements.
SADD users user1
SADD users user2
# Show the number of elements.
SCARD users
# Show all elements.
SMEMBERS users
# Show a random element.
SRANDMEMBER users 1

# Set a hash with four elements
HMSET mycache sku basic size c0
# Get all elements from the hash
HGETALL mycache
```

#### Azure Content Delivery Network

Azure CDN solution is caching static files globally as close to users as possible. This can be done in two caching modes: loading assets on demand (during first user request) or prepopulating assets to minimize delays with the first request.

Azure CDN leverage partner networks (such as Verizon) without storing data on its own servers. Unless Azure CDN is provisioned with standard tier or with **Azure Front Door**.

Caching statis files is trivial, but with Azure CDN dynamic files can also be cached when using the **dynamic site acceleration** (DSA) algorithm. This algorithm is using the following techniques.

- **Route optimization**. By checking latency from network and based on that choose the fastest and most reliable path.

- **TCP optimizations**. By optimizing TCP packet parameters and leveraging persistent connections.

- **Object prefetch**. Retrieve assets embedded in HTML page, while that page is rendered in the browser but before these assets are requested by the browser.

- **Adaptive image compression**. Applies standard JPEG compression when network is lagging.

When provisioning Azure CDN, it needs deployment of a CDN profile with one or more CDN endpoints. Location not required, this is set to *Global Network*. Requires an unique name and the product, which cannot be changed, like **Standard from Microsoft**, **Standard from Edgio**, or **Premium from Edgio**.

```bash
# For a CDN via Microsoft Azure, it requires a storage location.
# We use a Azure Blob Storage for this.
az group create --name "mjoy-rg" --location "westeurope"
az storage account create --name "mjoy-bs" --resource-group "mjoy-rg"
  --location "westeurope"
# Create the CDN profile.
az cdn profile create --name "mjoy-cdnp" --resource-group "mjoy-rg"
  --location "westeurope" --sku Standard_Microsoft
# Create and link a CDN endpoint.
az cdn endpoint create --name "mjoy-cdne" --profile-name "mjoy-cdnp"
  --resource-group "mjoy-rg"
  --origin mjoy-bs.blob.core.windows.net
  --origin-host-header mjoy-bs.blob.core.windows.net
  --origin-path "/files"
```

###### Caching rules

Caching rules are managed by Time To Live (TTL) values. These values can be obtained via the **Cache-Control** header. CDNs can override this cache control header with either the **Override** or **Bypass** behavior. When this header is not present, a default of 7 days is set for the Time To Live.

In Azure CDN you can configure **global caching rules** per endpoint, affecting all request of that endpoint.

Can also configure **custom caching rules** to match specific paths or extensions. These rules are processed in order and can override the *global caching rule* as well.

###### Purging cached content

Best practise is create a new version when data is updated at source, and not in the cache. But purging a cache specific path is available. Downside is that browsers and proxy servers can have it cached as well. And it can take up to 2 minutes before purging is completed.

#### Questions

1. What Azure Cache for Redis SKU does not provide an SLA?
   
   - *Basic*.

2. What Redis command is used to set TTL?
   
   - *`EXPIRE`. You can also use `TTL`, but than you will get the remaining time to live of the cached item.*

3. What is the differentce between a Basic and Premium SKU for Azure Cache for Redis?
   
   - *Premium supports clustering and data persistent during restarts.*

4. What protocol and port are used for connection to Azure Cache for Redis?
   
   - *TCP protocol over the ports 6379 and 6380.*

5. What CDN products are available with a Premium SKU for Azure CDN?
   
   - *Next to the Standard SKU, it has asset pre-loading, token authentication, and real-time monitoring.*

6. What are the three ways of controlling TTL in Azure CDN?
   
   - *Setting up a global caching rule, a custom caching rule and purging cached content. Other options, such as preloading and geo-filters are not linked to controlling TTL, but more making a resource available.*

## 10 Using Metrics and Log Data

Metrics and log data can be storaged, queried and exported within Azure, via several services.

- **Azure Monitor**
  
  - Monitoring hub that collects the data. Collect logs for 90 days before it is being overwritten. With 30 days being persisted. Can export the data to storage accounts.

- **Log Analytics**

- **Application Insights**
  
  - Similar to Google Analytics, where it can track user events on client-side applications. And server performance on server-side SDKs.

These services can be setup via CLI, but require an Azure Web App (or other service).

```bash
az group create --name "mjoy-rg" --location "westeurope"
az appservice plan create --name "mjoy-asp" --resource-group "mjoy-rg"
  --sku "B1"
# Create Azure Web App.
az webapp create --name "mjoy-wa" --resource-group "mjoy-rg"
  --plan "mjoy-asp"
# Create and configure Azure SQL.
az sql server create --name "mjoy-wa-sql" --admin-user "mjoyadmin"
  --admin-password "[STUPID-PASSWORD]" --resource-group "mjoy-rg"
az sql db create --name "mjoy-wa-db" --server "mjoy-wa-sql"
  --service-objective "Basic" --resource-group "mjoy-rg"
az sql server firewall-rule create --name "Allow connnect"
  --server "mjoy-wa-sql" --start-ip-address 0.0.0.0
  --end-ip-address 0.0.0.0 --resource-group "mjoy-rg"
# Create Azure Storage Account.
az storage account create --name "mjoysa" --resource-group "mjoy-rg"
# Create application settings for the Web App.
az webapp config appsettings set --name "mjoy-wa"
  --resource-group "mjoy-rg"
  --setting SqlConnection="[SQL-CONNECTIONSTRING]"
az webapp config appsettings set --name "mjoy-wa"
  --resource-group "mjoy-rg"
  --setting BlobConnection="[BLOB-CONNECTIONSTRING]"
```

It can occur that services in Azure have downtime due to issues on Azure side. This should never be lower than the SLA of the service. To calculate the SLA of services combined, you need to multiply the services SLA with each other. E.g.

- Azure Web App in Basic tier (99,95) * Azure SQL database (99,99) = 99,94%



NOT FEELING THE MONITORING SECTION OF THE BOOK. SKIPPING IT.



## 11 Implementing API Management

API Management (APIM) service is the layer before the web APIs, that handles communications between in coming requests and your APIs. Similar to a proxy instance. When using it, it requires the usage of a **Web Application Firewall** (WAF), such as Azure Front Door, to provide a security measurement. Several mohtly fixed tiers are available.

- Developer
  
  - No SLA. Single scalable unit. And 10 MB of cache.

- Basic
  
  - 99,95 SLA. Two scalable units. And 50 MB of cache.

- Standard
  
  - 99,95 SLA. Four scalable units. And 1 GB of cache.

- Premium
  
  - 99,99 SLA. With being deployed in several regions and up to 10 scalable units. And 5 GB of cache.

But when the cost are to high, you can also use the Consumption tier, which is a pay-as-you-use tier. This tier only allows one instance per subscription. It does not provide an internal cache, VNet integration, multi-deployments and self-hosted gateways for connection to the on-premises backend. So, it good for setting up a POC.

To demonstrate the API Management service we need a simple web API, which we can generate with `dotnet`.

```bash
# First create the resource group with the App Service Plan.
az group create --name "mjoy-rg" --location "westeurope"
az appservice plan create --name "mjoy-asp" --resource-group "mjoy-rg"
  --sku "B1"
# Than create Web App resource, with Swagger enabled.
az webapp create --name "mjoy-wa" --resource-group "mjoy-rg"
  --plan "mjoy-asp"
az webapp config appsettings set --name "mjoy-wa"
  --resource-group "mjoy-rg"
  --settings "ASPNETCORE_ENVIRONMENT=Development"
# And create a simple web API within a solution.
dotnet new webapi --name "DemoCatalog"
dotnet new sln --name "DemoCatalog"
dotnet sln add .\DemoCatalog\DemoCatalog.csproj
# Publish the web API and manually ZIP the generated resources.
dotnet publish 'DemoCatalog' -o 'publish'
cd .\publish
# Finally deploy the ZIP file to the web API.
az webapp deploy --name "mjoy-wa" --resource-group "mjoy-rg" --type zip
  --src-path "DemoCatalog.zip"
```

When setting up a API Management, the following is required.

- **Unique name**, which can be used as the Fully Qualified Domain Name (FQDN).

- **Organization name** and **administrators email**, to setup the developer portal.

- **Preferred location**, wich is best to use the resource group location.

- **Pricing tier**.

You can also setup.

- **Scaling units**.

- **Managed identities**, for communication with Azure resources that requires authorization and authentication (e.g. Key Vault).

- **VNet integration**, to enable internal firewall for external connections.

- **Protocol settings**, to allow setup of different TLS versions.

```bash
# Here we create the actual API Management service.
az apim create --name "mjoy-apim" --location "westeurope"
  --resource-group "mjoy-rg" --publisher-name "mjoy-apim"
  --publisher-email "mjoy-apim@demo.com" --sku-name Consumption
# And we can link our Web API to our API Management service.
az apim api import --service-url "https://mjoy-wa.azurewebsites.net/"
  --display-name "weather-api" --api-id "weather-api" --path "weather-api"
  --specification-url "https://mjoy-wa.azurewebsites.net/swagger/v1/swagger.json"
  --specification-format "OpenApiJson" --resource-group "mjoy-rg"
  --service-name "mjoy-apim"
```

#### Products and subscriptions

Product is a set of logically grouped APIs. Subscription is a key provided for a customer, who wants to call APIs within the product. Product can have more than one subscription.

Three types of products exist.

- Free

- Billable

- Trail

When using a subscription, the header **Ocp-Apim-Subscription-Key** is required for each API request. Containing the subscription key.

#### Authentication

The developer portal can be access by unauthenticated users and users with an account. An account requires an email and password, but can be setup via several identity providers (e.g. Azure Active Directory). This account is than linked a subscription key.

A user account can be linked to a group. There are three immutabke groups. But a custom group can also be setup by an administrator.

- **Administrators**, with full permissions on products and APIs.

- **Developers**, with authenticated users who can test APIs based on the subscription.

- **Guests**, for unauthenticated users who can view APIs but cannot call them.

#### External cache

When build-in cache of the choosen price tier is not sufficient, it is possible to link an external cache (such as *Azure Cache for Redis*) to the setup API Management service. It is required to provide a connection string supported by `StackExchange.Redis`. This method is ideal for when the Consumption tier is selected, which does not have a build-in cache in API Management.

#### Advanced policies

Can setup a policy per operation, but also a global policy. Policies are defined in XML format. When defining a global policy, it is required to setup the policy per operation and refer to the global policy via the `<base>` element.

Several sections are mandatory in a policy.

- `<inbound>`

- `<backend>`

- `<outbond>`

- `<on-error>`

Other elements are also possible, such as.

- `<mock-response>`, to mock an API response containing the HTTP status code and content type. Must be setup in the `<inbound>` element.

- `<return-response>`, to setup a response. Must contain the elements `<set-status>`, `<set-header>` and `<set-body>`. Must be setup in the `<inbound>` element.

- `<cache-lookup>`, to the setup the caching. Must be setup in the `<inbound>` element. But requires the `<cache-store>` element, to configure the cache duration.

- `<rate-limit>`, to enable request throttling. Must be setup in the `<inbound>` element.

- `<quote-by-key>`, also to enable request throttling. Must be setup in the `<inbound>` element.

#### Questions

1. Whats is the REST standard and what HTTP verbs are used in REST?
   
   - *REST is the abbreviation for **Representational State Transfer**, which implies the HTTP verbs **GET**, **POST**, **PUT** and **DELETE**.*

2. Doe APIM support horizontal scaling?
   
   - *Yes, but only in the Basic, Standard, and Premium pricing tier.*

3. What are the three ways to connect existing web APIs from your Azure subscription to the APIM server?
   
   - *From **Swagger**, **Web Application Description Language** (WADL) or **Web Services Description Language** (WSDL).*

4. Are there any disadvantages to using a consumption-based pricing for APIM?
   
   - *Yes. This tier only allows one instance per subscription. It does not provide an internal cache, VNet integration, multi-deployments and self-hosted gateways for connection to the on-premises backend.*

5. What is the policy expression syntax based on?
   
   - *Based on XML.*

## 12 Developing Event-Based Solutions

Several services persist in the Azure domain to setup event-based solutions

- Azure Event Hubs

- Azure IoT Hubs

- Azure Notification Hubs

- Azure Event Grid

All of them implement the **pubished-subscriber** pattern.

#### Azure Event Hubs

Setting up Azure Event Hubs require the setup of an **Event Hub namespace**. Which is a virtual server that needs to be deployed in Azure. A FQDN will be provided after setting up an Event Hub namespace. And accepts connections with **MQ Telemetry Transport** (MQTT), **Advanced Message Queueing Protocol** (AMQP), **AMQP over WebSockets** and **HTTPS**. 

Following price tiers can be chosen from.

- **Basic**
  
  - Retention period of 1 day. No dynamic partitin scaling, capturing events, and VNet integration. Up to 40 scalable units.

- **Standard**
  
  - Retention period of 7 days. Can capture events and allowes VNet integration. Up to 40 scalable units.

- **Premium**
  
  - Retention period of 90 days. Allows all features without any throughput limits.

- **Dedicated**
  
  - Same as *Premium* tier. Provisioned in an exclusive single-tenant environment.

Events within an Event Hub will persist until it is being consumed by a consumer or the retention period has passed. After that the copy will only exists in the blob for analysis purposes.

Consumers can be setup within a **consumer group**. By default the **$Default** consumer group will be used when none is provided. Events can be tagged with a consumer group, making it only consumable for consumers within that consumer group.

Authentication and authorization are handled via **shared access signature** (SAS) tokens. Each SAS token can have the **listen**, **send**, and **manage** rights. Where the **manage** right is for listening and sending events.

```bash
# Create the resource group and a blob storage account.
az group create --name "mjoy-rg" --location "westeurope"
az storage account create --name "mjoysa" --resource-group "mjoy-rg"
az storage account keys list --account-name "mjoysa" --query [0].value
az storage container create --name "capdate" --public-access "blob"
  --account-name "mjoysa" --account-key "[KEY]"
# Create the Event Hub namespace.
az eventhubs namespace create --name "mjoy-ehn"
  --resource-group "mjoy-rg" --location "westeurope" --sku Standard
# Create the Event Hub.
az eventhubs eventhub create --name "mjoy-eh" --namespace-name "mjoy-ehn"
  --resource-group "mjoy-rg"
# Enable event capturing in storage account.
az eventhubs eventhub update --name "mjoy-eh" --namespace-name "mjoy-ehn"
  --enable-capture --destination-name EventHubArchive.AzureBlockBlob
  --storage-account "mjoysa" --blob-container capdate
  --resource-group "mjoy-rg"
az eventhubs eventhub authorization-rule create --eventhub-name "mjoy-eh"
  --namespace-name "mjoy-ehn" --name apps --rights Listen Send
  --resource-group "mjoy-rg"
# Get the connection strings. Can also be retrieved from Azure Portal.
az eventhubs eventhub authorization-rule keys list --name apps
  --eventhub-name "mjoy-eh" --namespace-name "mjoy-ehn"
  --resource-group EventHubDemo-RG --query primaryConnectionString
az storage account show-connection-string --name "mjoysa"
  --resource-group EventHubDemo-RG --query connectionString
```

#### Azure IoT Hub

Azure IoT Hub is designed for consuming streaming telemetry and managing of IoT devices. It can communicate with IoT devices via TCP, **MQ Telemetry Transport** (MQTT), **Advanced Message Queueing Protocol** (AMQP), and **HTTPS**. Performance of Azure IoT Hub is the most valuable metric.

Following price tiers can be chosen from.

- **Basic (B1-B3)**
  
  - Allows to ingest 0,4 MB to 300 MB of messages, with all described protocols. Does not support cloud-to-device messages.

- **Standard (S1-S3)**
  
  - Similar to Basic tier. Does support cloud-to-device messages. Supports Azure IoT Edge, for us to orchestrate containers and their configuration on the devices itself.

- **Free (F1)**
  
  - Similar to Standard, but only one instance per subscription.

To register a device, we should use the **Device Provisioning Service** (DPS). This service handles authentication and authorizaion either via a shared access signature (SAS) or a certificate. Which both can be revoked when the device is stolen or no longer needed.

In Azure IoT Hub no need to provision a namespace. Only require a pricing tier and unique name.

```bash
# Add IoT and streaming analytics extensions to our subscription.
az extension add --upgrade --name azure-iot
az extension add --upgrade --name stream-analytics
# Setup the resource group and storage account.
az group create --name "mjoy-rg" --location "westeurope"
az storage account create --name "mjoysa" --resource-group "mjoy-rg"
az storage container create --account-name "mjoysa" --name state
  --account-key "[SA-ACCOUNT-KEY]" --auth-mode key
# Creat the IoT hub.
az iot hub create --name "mjoy-iothub" --resource-group "mjoy-rg"
  --sku S1
az iot hub policy show --name "iothubowner" --hub-name "mjoy-oithub"
  --query primaryKey
# Setup stream analytics.
az stream-analytics job create --job-name "mjoy-iotjob"
  --resource-group "mjoy-rg" --output-error-policy "Drop"
  --data-locale "en-US" --functions "[]" --inputs "[SOME-INPUT-FORMAT]"
  --outputs "[SOME-OUTPUT-FORMAT]"
az stream-analytics transformation create --name Transformation
  --job-name "mjoy-iotjob" --resource-group "mjoy-rg"
  --streaming-units "1"
  --saql "SELECT * INTO outblob FROM inhub WHERE humidity > 70"
az stream-analytics job start --job-name "mjoy-iothub"
  --resource-group "mjoy-rg" --output-start-mode JobStartTime
# Provision a device
az iot hub device-identity create --name "mjoy-iotdevice"
  --device-id "vdevice"
az iot hub device-identity connection-string show --device-id "vdevice"
  --name "mjoy-iotdevice" --query connectionString
```

The last command returns the connection string of the device we have registered in our Azure IoT Hub. This connection string can be setup at [Raspberry Pi Azure IoT Web Simulator (azure-samples.github.io)](https://azure-samples.github.io/raspberry-pi-web-simulator/), where we can mock a IoT device.

#### Azure Event Grid

TODO

#### Questions

1. What is a namespace?
   
   - *Virtual server required to provision an Event Hub.*

2. How is Azure IoT Hub different from Event Hubs?
   
   - *Azure IoT Hub is specifically designed to consume streaming telemetry of and managing IoT devices.*

3. How long does an event persist in Event Hubs?
   
   - *An event persist until it is being consumed by a consumer or the retention period has passed. The retention period depends on the selected pricing tier.*

4. What service helps you connect Azure Functions and Event Hubs?
   
   - TODO

5. What type of authentication can be used to connect IoT devices to Azure IoT Hub?
   
   - *Two types, a shared access signature (SAS) or a certificate. Which both can be revoked when the device is stolen or no longer needed.*

6. What is a possible misconfiguration for an event handler that will not allow it to receive all events send by Event Grid?
   
   - TODO
