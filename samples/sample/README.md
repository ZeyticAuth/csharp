# ZeyticAuth ASP.NET Core sample project

This sample project shows how to use the [ZeyticAuth ASP.NET Core authentication middleware](../src/ZeyticAuth.AspNetCore.Authentication/) to authenticate users with ZeyticAuth.

## Prerequisites

- .NET 6.0 or higher
- A [ZeyticAuth Cloud](https://zeytic.com/) account or a self-hosted ZeyticAuth
- A ZeyticAuth traditional web application created

### Optional

- Set up an API resource in ZeyticAuth

If you don't have the ZeyticAuth application created, please follow the [⚡ Get started](https://docs.zeytic.com/docs/tutorials/get-started/) guide to create one.

## Configuration

Create an `appsettings.Development.json` (or `appsettings.json`) with the following structure:

```jsonc
{
  // ...
  "ZeyticAuth": {
    "Endpoint": "https://<your-zeytic-endpoint>/",
    "AppId": "<your-zeytic-app-id>",
    "AppSecret": "<your-zeytic-app-secret>"
  }
}
```

If you need to test API resource, add the `Resource` key:

```jsonc
{
  // ...
  "ZeyticAuth": {
    // ...
    "Resource": "https://<your-api-resource-indicator>"
  }
}
```

## Run the sample

```bash
dotnet run # or `dotnet watch` to run in watch mode
```
