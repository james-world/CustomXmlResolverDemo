# Overview

This is a spike to test a custom XmlResolver that can pull files from Azure Blob Storage.

The demo incldues a sample parent XLST file `main.xslt` that includes a child XSLT file `common.xslt`. The custom XmlResolver is used to pull both files from Azure Blob Storage and transform a locally stored `books.xml` file into HTML.

The files are in the Example folder in src/Demo.

## Instructions

Before you start, you can either run a mock emulator for Azure Storage or use a real Azure Storage account.

### Azurite Emulator

Run `docker compose up --build` to start the Azure Storage emulator and automatically upload the `common.xslt.xml` and `main.xslt.xml` files to the emulator.

OR

### Real Azure Storage Account

You need to set the dotnet user secret for the `AzureBlobXmlResolver:ConnectionString` key to the connection string of the Azure Storage account:

```bash
dotnet user-secrets --project src/Demo set AzureBlobStrage:ConnectionString "<blob storage connection string>"
```

You need to upload the `common.xslt` and `main.xslt` files to an Azure blob storage container called `xmltest`.

It would look like:

![FilesInBlobStorage](FilesInBlobStorage.png)

### Run the Demo

With blob storage ready by one the above methods, you can run the demo.

From the command line, run `dotnet run --project src/Demo`.