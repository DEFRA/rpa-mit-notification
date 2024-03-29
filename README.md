# Introduction

This is the MIT Notifications project. See the following [wiki](https://dev.azure.com/defragovuk/DEFRA-EST/_wiki/wikis/DEFRA-EST/8604/Manual-Invoice-Template) for information about the project.

# Getting Started

Clone this [repo](https://github.com/DEFRA/rpa-mit-notification).

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
- rpa-mit-invoicenotification
- rpa-mit-invoicenotification-poison
- rpa-mit-events

The storage acount also needs the following table created:
- rpamitinvoicenotification (no hyhens are allowed in a table name)

Install Azure Storage Explorer to create the above queues/tables in a storage account.

For local development ensure Azurite is installed. This is installed as part of VS2002.

Install the VSCode Azurite extension to start / stop Azurite table, blob, file, queue services.

# Build and Test

The easiest way to build the project is with VS2022. It should download all required nuget dependencies.

Run the tests using the VS2022 Test Explorer.

To run locally in Docker, run:
```docker-compose up```

(Homepage will be accessible on http://localhost:3000 to prove the Function App is running)

Post a message to the invoicenotification queue. Here is a sample message:

```
{
"Action": "approval",
"Data":
    {
    "invoiceId": "12345",
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

If using an API key that is a 'team' key, you will need to be added to the 'team' by whoever owns the API key in order to send emails to yourself.

# Code check-in

Prior to committing changes run the following command from a cmd line within the solution folder: dotnet format whitespace --verbosity n

Install the VS2022 extension SonarLint. Prior to commiting changes check the Messages window for any code smells / issues that would cause the build pipeline to fail.

# Cloud setup
To create the cloud infrastructure in AKS follow the guidlines on the FC Platform section of the DEFRA-EST Wiki. 



