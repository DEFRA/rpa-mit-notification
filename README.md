# Introduction 

This is the MIT Notifications project. See the following [wiki](https://dev.azure.com/defragovuk/DEFRA-EST/_wiki/wikis/DEFRA-EST/8604/Manual-Invoice-Template) for information about the project.

# Getting Started

Clone this [repo](https://defragovuk@dev.azure.com/defragovuk/DEFRA-EST/_git/RPA.MIT.Notification).

# Local setup

No sensitive information is stored in the local.settings.json. 

When developing locally values may be set via `dotnet user-secrets`.

The simplest way to add secrets to the local store via Visual Studio by right clicking on the
`RPA.MIT.Notification.Function` project in the Solution Explorer, and selecting 'Manage User Secrets'.

Alternatively save the values into a `secrets.json` file, i.e. 
```
{
  "NotifyApiKey": "--SECRET-VALUE--"
}

```
then load via the command line:
```
cat ./secrets.json | dotnet user-secrets set
```

Example values can be found on the project[Wiki](https://dev.azure.com/defragovuk/DEFRA-EST/_wiki/wikis/DEFRA-EST/7758/AzureAD-Sample-settings)

# Storage Account Setup

The project requires a storage account and the following queues created:
- invoicenotification
- invoicenotification-poison

Install Azure Storage Explorer to create the above queues in a storage account.

For local development ensure Azurite is installed. This is installed as part of VS2002.

# Build and Test

The easiest way to build the project is with VS2022. It should download all required nuget dependencies.

Run the tests using the VS2022 Test Explorer.

Post a message to the invoicenotification queue. Here is a sample message:

```
{
"Action": "approval",
"Data":
    {
    "invoiceId": "123456789",
    "link": "https://google.com",
    "name": "Steve Dickinson",
    "schemeType": "bps",
    "value": "250"
    },
"Id": "123456789",
"Scheme": "bps"
}
```

# Gov Notify

You will need a [Gov Notify account] (https://www.notifications.service.gov.uk/) to view the Notify templates that are used. Each template has an id. Each template id is mapped via the Action property of the incoming message.

# Code check-in

Prior to committing changes run the following command from a cmd line within the solution folder: dotnet format whitespace --verbosity n

Install the VS2022 extension SonarLint. Prior to commiting changes check the Messages window for any code smells / issues that would cause the build pipeline to fail.

# Cloud setup
To create cloud infrastructure to run the project the following commands may be run:

set your subscription so all activities are carried out on the right subscription: 
```
 az account set --subscription "eeeeeeee-eeee-eeee-eeee-eeeeeeeee"
 ```
create a resource group
```
az group create --name "my-resource-group" --location uksouth
```
create key vault
```
az keyvault create --name est-mit-kv-dev --resource-group my-resource-group --location uksouth
```
create storage account, function app, and assign identity
```
az storage account create --name est-mit-storage-dev --resource-group my-resource-group --location uksouth
az functionapp create --name est-mit-notifications-dev  --resource-group my-resource-group --storage-account est-mit-storage-dev --location uksouth
az functionapp identity assign --resource-group my-resource-group --name est-mit-notifications-dev
```

copy principal ID from above output and give permission to key vault
```
az keyvault set-policy --secret-permissions get list --name est-mit-kv-dev  --object-id **principalID-from-above
```

Key may be added via the command line, i.e.:

```
az keyvault secret set --vault-name "est-mit-kv-dev" --name "NotifyApiKey" --value "--api-key--"
```



