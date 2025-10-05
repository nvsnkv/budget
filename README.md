# NV's budget

Yet another budget tracking tool. Successor of [flow](https://github.com/nvsnkv/flow2). The main features are:
* Import data from any CSV list of operations provided by your banks. `budget` provides uniform way of storing this data
* Find and mark transfers between your accounts within a budget to exclude them from "incomes" and "withdraws"
* Tag operations in a way _you_ need with powerful tagging criteria
* Build calendar-like aggregation of your operations to track your expences month-to-monht

## How to use it?

### Prerequisites
Budget still needs a PostgreSQL database to store the data. You can use `docker-compose.yml` from [src/Hosts](./src/Hosts/) folder to spin up new database but please make sure you changed password!
You'll also need a [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) for your OS.

Optionally, you'll need a Rider IDE if you would like to build it on your own (publish script is currently rider-specific). Any other IDE for .NET will work, but you'll need to figure out how to publish on your own.

### Installation

At some point I'll start publishing releases to github, but for now you'll need to clone repository and build [./src/Hosts/NVs.Budget.Hosts.Console](https://github.com/nvsnkv/budget/blob/console/src/Hosts/NVs.Budget.Hosts.Console/NVs.Budget.Hosts.Console.csproj) project - it's an application entry point. Once compiled, you need to update application settings (define a connection string to database at least) and then you should be ready to explore `budget` features.
