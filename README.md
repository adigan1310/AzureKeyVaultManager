# AzureKeyVaultManager

Cross-Platform utility to automate bulk import/export of keys from Azure Key Vault.

Developed using Electron.Net Application following onion architecture. Utility exports the keys into JSON file and bulk import is done through reading values from JSON file.

Steps to Run:
- Open solution in visual studio code
- Navigate to AzureKeyManager folder
- Type dotnet restore (to restore all packages)
- Type dotnet electronize start (to run the application)
- Type dotnet electronize build /target *osname* (to package it into standalone application. osname: win or macos or linux)

Modules:
Configuration: Allows you to store/modify configuration details of key vault like client-id, secret-key, URI locally.
Export Keys  : Allows you to export keys from specified Key Vault URL insto JSON File.
ImportKeys   : Allows you to import keys from a JSON file into a specified Key Vault. Values can be modified directly on the screen.

