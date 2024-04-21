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

One of goals of `budget` is to give you an answer to the question "how much money did I _actually_ spend?". To do so, it detect cross-account transfers so you can exclude them from calculations.

List of trasfer criteria looks similar to input file parsing options - it consists of list of name - configuration pairs. Each configuration consist of two fields: `Accuracy` tells how likely given pair of operations is a transfer, and `Criterion` provides the rule itself.

The rules are highly-configurable. They use [Dynamic LINQ](https://dynamic-linq.net/) under the hood, so it's better to think that each criterion is an C# boolean statement that can use variables `l` and `r` for the pair of operation beign tested.

The example below demonstate a rule [bundled with this app](./src/Hosts/NVs.Budget.Hosts.Console/appsettings.json#L18):
```
"Transfers": {
    "Instant transfer": {
      "Accuracy": "Exact", "Criterion": "l.Amount.IsNegative() && r.Amount.IsPositive() && l.Amount.Abs() == r.Amount.Abs() && l.Description == r.Description && l.Account != r.Account && (l.Timestamp - r.Timestamp).Duration() < TimeSpan.FromMinutes(2)"
    }
  },
```
The rule "Instant transfer" ensures that:
* the left operation (`l`) is a withdraw (`l.Amount.IsNegative()`),
* right one (`r`) is an income (`r.Amount.IsPositive()`),
* amounts, excepting the sign, are equal (`l.Amount.Abs() == r.Amount.Abs()`)
* descriptions are the same (`l.Description == r.Description`), 
* accounts are different (`l.Account != r.Account`) 
* and operations happened within 2 minutes (`(l.Timestamp - r.Timestamp).Duration() < TimeSpan.FromMinutes(2)`).

When creating your own rules, you can use all properties from [TrackedOperation](./src/Application/NVs.Budget.Application.Contracts/Entities/Accounting/TrackedOperation.cs) class for both left and right optations.

#### Configuration: Tagging rules

Last, but not least - you can provide tagging rules.
Collection of rules consist of tag name - criterion pairs, where criterion is a [Dynamic LINQ](https://dynamic-linq.net/) expression similar to transfers criterion. The only difference that it accepts only one variable `o` for the operations beign tagged.

[Example bundled with an app](./src/Hosts/NVs.Budget.Hosts.Console/appsettings.json#L24) will tag all incomes:
```
o.Amount.Amount > 0
```

### First steps (console app)

#### Updating the database

Run `bugdet admin migrate-db` command - it will apply all migrations to the database and you would be good to go! Running this command after updating `budget` to a new version is important, don't forget to do it!

#### Creating an owner

`budget` distinguishes _users_ from _owners_ . Owner is someone who owns one or more _Accounts_ and can manage operations, rename or even delete an account, while the user is just someone who runs the commands in command line.

That's why you need to create an association between your user (identified as an OS user) and owner record.

Run `budget owners self-register` to create such association. After that, you will be able to perform import.

#### Running import

After you wrote the congiration, applied database migration and created an owner record you're ready to import operation from as many accounts as you have!
Place all CSV files with operations you want to import to a single folder, and run `budget ops import -d %PATH_TO_THE_FOLDER%`. Application will iterate through the files, find appropriate configuration for each of them, creates new accounts (only if you provide `--register-accs` flag), detect the transfers and save transfers with confidence level greater or equal to `Exact` (you can lower this threshold by providing `--transfers-confidence` parameter. Valid options are `Exact`, `Likely`).

