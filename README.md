# MVC XML Configuration Sample

This repository contains an ASP.NET Core MVC application that demonstrates how to load, display, and edit application settings stored in an XML file with validation against an XSD schema.

## Features

- Reads configuration data from `App_Data/settings.xml` using a strongly-typed model.
- Displays the current configuration on the **Settings** page.
- Provides a form to edit the XML configuration values.
- Validates saved changes against `App_Data/settings.xsd` before persisting them.

## Getting started

1. Install the [.NET 7 SDK](https://dotnet.microsoft.com/download) if it is not already available on your machine.
2. Restore and run the application:

   ```bash
   dotnet restore MvcXmlConfigApp/MvcXmlConfigApp.csproj
   dotnet run --project MvcXmlConfigApp/MvcXmlConfigApp.csproj
   ```

3. Navigate to `https://localhost:5001/Settings` (or the URL shown in the console) to view and edit the configuration.

## XML Schema validation

When the form is submitted, the service layer serializes the updated settings back to XML, validates the XML against `settings.xsd`, and only writes the file if the schema validation succeeds. Any validation errors are surfaced back to the user via model state errors.
