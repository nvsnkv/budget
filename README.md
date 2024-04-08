# NV's budget

Yet another budget tracking tool. Successor of [flow](https://github.com/nvsnkv/flow2). The main features are:
* Import data from any CSV list of operations provided by your banks. `budget` provides uniform way of storing this data
* Find and mark transfers between your accounts to exclude them from "incomes" and "withdraws", if that was a transfer from your account to another your account
* Tag operations in a way _you_ need with powerful tagging criteria
* Build calendar-like aggregation of your operations to track your expences month-to-mont

## How to use it?

### Prerequisites
Budget still needs a PostgreSQL database to store the data. You can use `docker-compose.yml` from [src/Hosts](./src/Hosts/) folder to spin up new database but please make sure you changed password!
You'll also need a [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) for your OS.

Optionally, you'll need a Rider IDE if you would like to build it on your own (publish script is currently rider-specific). Any other IDE for .NET will work, but you'll need to figure out how to publish on your own.

### Installation

At some point I'll start publishing releases to github, but for now you'll need to clone repository and build [./src/Hosts/NVs.Budget.Hosts.Console](https://github.com/nvsnkv/budget/blob/console/src/Hosts/NVs.Budget.Hosts.Console/NVs.Budget.Hosts.Console.csproj) project - it's an application entry point. Once compiled, you need to update application settings (define a connection string to database at least) and then you should be ready to explore `budget` features.

### Configuration

Application uses [.NET Configuration](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) with JSON files, command line and environment variables providers. Configuration is quite bulky, so I recommend the following way:
1. Define environment variable `BUDGET_CONFIGURATION_PATH` with the path to the folder where you will store all configuration specific to _your_ installation. That should be: 
    * Connection strings
    * Input file parsing configuration
    * Transfer detection criteria
    * Tagging rules
2. Create one or more JSON files with the configs. Define the connection string first. The sample below contains default connection string that matches instance created by `docker-compose.yml` (any you obviously must change credentials for your installation!):
```
"ConnectionStrings": {
    "BudgetContext": "User ID=postgres;Password=postgres;Host=localhost;Port=20000;Database=budgetdb;" 
}
```
3. Refer to the docs below how to configure remaining important options

#### Configuration: Input files parsing

`budget` allows to store following information for every operation:
* `Timestamp` when its happened
* `Amount` of this operation (like '$10')
* `Description` 
* `Attributes` - list of key-value pairs for anything else you want to track (MCC codes, original categories from your bank, geotags and so on...)
* `Tags` - list of tags that you can assign by retagging stored transaction (see below)
* `Account` information, which consist of
    * account `Name`
    * `Bank` name

Different banks provides this info in a different way, that's why `budget` needs an explicit configuration that tells how to threat each input file you're providing. To configure that, you'll need to provide `CsvReadingOptions`. Let's review the sample below:
```
"CsvReadingOptions": {
    "validFile.csv": {
      "Timestamp": "{0}",
      "Amount": "{7}",
      "CurrencyCode": "{8}",
      "Description":"{6}",
      "Account.Name": "{2} ({12})",
      "Account.Bank": "The Bank",
      "Attributes": {
        "MCC": "{11}",
        "Category": "{10}"
      },
      "ValidationRules": {
        "not a header": { "FieldConfiguration": "{0}", "Condition": "NotEquals", "Value": "Date" }
      }
    },
    "my_bank.operations.*.csv" : {
        ...
    }
}
```
`CsvReadingOptions` contains a list of [pattern](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions)-configurations pairs. Application tests the filename agains all the patterns provided until the first success. For each pattern we need to provide the full set of fields(`Timestamp`, `Amount`, `CurrencyCode` etc).

The `{0}` is a substitution, that tells which cell index (zero-based) should be used to read this value. You can combine several cells into single value or even provide hardcoded value.

The `Attributes` field define which attributes will be populated and what would be the value. It can be empty if you don't need anything.

`ValidationRules` define which row to parse. You can exclude header row, or "Failed/Cancelled" operations. Application will get the value specified by `FieldConfiguration` and test it agains `Condition` and `Value`. In example above it will process the row only if the first cell contains value, which is not equal to the word `Date`.

#### Configuration: Transfers detection

TBD

