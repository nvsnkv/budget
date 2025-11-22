# NV's budget

Yet another budget tracking tool. Successor of [flow](https://github.com/nvsnkv/flow2). The main features are:
* Web UI to manage data!
* Import data from any CSV list of operations provided by your banks. `budget` provides uniform way of storing this data
* Find and mark transfers between your accounts within a budget to exclude them from "incomes" and "withdraws"
* Tag operations in a way _you_ need with powerful tagging criteria
* Build calendar-like aggregation of your operations to track your expences month-to-monht

## How to use it?
0. Configure authentication service in [Yandex OAuth](https://oauth.yandex.ru/). Service currently supports only yandex oauth provider.
1. Host budget web services (client bundle and server-side app). Deployment options are described below.
2. Configre budget settings (import options, tagging and transfer ctriteria)
3. Configure aggregation rules (logbook criteria)

Starting from this point you're ready to use service:
4. Import new operations
5. Handle possible duplicates and transfers
6. Build and explore agregated expenses

### Hosting

The application consists of two main components that need to be hosted:

1. **Web Server** (`NVs.Budget.Hosts.Web.Server`): .NET 8.0 ASP.NET Core API server
2. **Web Client** (`NVs.Budget.Hosts.Web.Client`): Angular application served as static files

#### Prerequisites

- PostgreSQL 17+ database
- .NET 8.0 SDK (for building)
- Node.js 20+ and npm (for building client)
- Docker (optional, for containerized deployment)

#### Configuration

The server requires the following configuration (via environment variables or `appsettings.json`):

- **Connection Strings**:
  - `ConnectionStrings:IdentityContext` - PostgreSQL connection string for identity/authentication data
  - `ConnectionStrings:BudgetContext` - PostgreSQL connection string for budget data
- **Authentication**:
  - `Auth:Yandex:ClientId` - Yandex OAuth client ID
  - `Auth:Yandex:ClientSecret` - Yandex OAuth client secret
- **Frontend**:
  - `FrontendUrl` - URL where the client application is hosted
  - `AllowedOrigins` - Semicolon-separated list of allowed CORS origins

#### Docker Deployment

Both server and client have Dockerfiles for containerized deployment:

- **Server**: Exposes ports 7237 (HTTPS) and 5153 (HTTP)
- **Client**: Exposes ports 8080 (HTTP) and 8081 (HTTPS)

Build and run:
```bash
# Build server
docker build -f src/Hosts/NVs.Budget.Hosts.Web.Server/Dockerfile -t budget-server .

# Build client
docker build -f src/Hosts/NVs.Budget.Hosts.Web.Client/Dockerfile -t budget-client .

# Run with docker-compose (create your own compose file)
```

#### Development Setup

For local development, see [DEVELOPMENT.md](DEVELOPMENT.md) for detailed setup instructions.

#### Database Migration

After deployment, run database migrations:
```bash
GET /admin/patch-db
```

This endpoint applies all pending migrations to both identity and budget databases.

### Budget settings

Each budget has three types of settings that control how operations are processed:

#### 1. File Reading Settings (CSV Import Configuration)

Configure how CSV files from your banks are parsed. Each setting is associated with a file pattern (regex) and includes:

- **Culture**: Locale for parsing numbers and dates (e.g., `en-US`, `ru-RU`)
- **Encoding**: Text encoding of the CSV file (e.g., `UTF-8`, `Windows-1251`)
- **DateTime Kind**: How to interpret date/time values (`Local`, `Utc`, `Unspecified`)
- **Field Mappings**: Maps CSV column indices/names to operation properties:
  - `Amount` - Transaction amount
  - `Currency` - Currency code
  - `Timestamp` - Transaction date/time
  - `Description` - Transaction description
  - `Account` - Account identifier
- **Attribute Mappings**: Maps CSV columns to custom operation attributes
- **Validation Rules**: Rules to validate and filter CSV rows:
  - Condition: `Equals` or `NotEquals`
  - Field: Column to check
  - Value: Expected value

Example: Configure a setting for files matching `.*sberbank.*\.csv` with specific field mappings for Sberbank's CSV format.

#### 2. Tagging Criteria

Define rules that automatically assign tags to operations based on conditions. Each criterion consists of:

- **Tag Expression**: Expression that computes the tag name from operation properties
  - Example: `o => o.Description.Contains("Grocery") ? "Food" : "Other"`
- **Condition**: Boolean expression that determines when to apply the tag
  - Example: `o => o.Amount.Amount < 0` (only for expenses)

Tagging criteria are evaluated during import and update operations. Operations can have multiple tags.

#### 3. Transfer Detection Criteria

Define rules to automatically detect transfers between accounts. Each criterion includes:

- **Criterion Expression**: Binary predicate that matches a source (withdraw) and sink (income) operation
  - Example: `(source, sink) => source.Amount.Amount == -sink.Amount.Amount && source.Timestamp.Date == sink.Timestamp.Date`
- **Accuracy**: Confidence level of the match
  - `Exact` (100%) - High confidence, exact match
  - `Likely` (70%) - Probable match, may require review
- **Comment**: Optional description for the transfer

When a transfer is detected, both operations are tagged with `Transfer`, `Source`, or `Sink` tags and excluded from income/expense calculations.

### Aggregation rules (Logbook Criteria)

Logbook criteria define how operations are grouped and aggregated for expense tracking. Criteria form a hierarchical structure where each level can have subcriteria for further grouping.

#### Criterion Types

1. **Tag-Based Criterion**
   - Matches operations by tags
   - Types:
     - `Including`: Operation must have ALL specified tags
     - `OneOf`: Operation must have AT LEAST ONE of the specified tags
     - `Excluding`: Operation must NOT have any of the specified tags
   - Example: Group all operations tagged with "Food" or "Restaurant"

2. **Predicate-Based Criterion**
   - Matches operations using a boolean expression
   - Example: `o => o.Amount.Amount < 0 && o.Timestamp.Month == DateTime.Now.Month`
   - Useful for complex filtering logic

3. **Substitution-Based Criterion**
   - Groups operations by a computed value
   - The substitution expression returns a string that becomes the group name
   - Example: `o => o.Timestamp.ToString("yyyy-MM")` groups by month
   - Automatically creates subcriteria for each unique value

4. **Universal Criterion**
   - Matches all operations
   - Can be used as a root criterion or with subcriteria for grouping
   - When used with subcriteria, operations are distributed to matching subcriteria

#### Hierarchical Structure

Criteria can be nested to create multi-level groupings:

```
Universal (all operations)
├── Tag-Based: "Food" (food-related expenses)
│   ├── Substitution: Month (group by month)
│   └── Tag-Based: "Restaurant" (restaurant expenses)
└── Tag-Based: "Transport" (transportation)
    └── Substitution: Month
```

This structure allows you to build calendar-like views where operations are grouped by category and time period.

#### Usage

Logbook criteria are used when building aggregated statistics:
- Operations are matched against the root criterion
- Matching operations are registered in the logbook
- Operations are then matched against subcriteria for further grouping
- Each level maintains aggregated statistics (count, total amount, etc.)
- You can specify date ranges and additional filter criteria when querying the logbook

### Import

Import operations from CSV files exported by your banks. The import process uses the file reading settings configured for your budget to parse the CSV format.

#### Prerequisites

Before importing, ensure you have:
1. **Configured file reading settings** for your budget (see [Budget settings](#budget-settings))
2. A CSV file exported from your bank

#### Import Process

1. **Navigate to the import page** for your budget
2. **Select a CSV file** to import
3. **Optional: Specify file pattern** - If you have multiple reading settings, provide a regex pattern to match the correct one (e.g., `.*sberbank.*\.csv`)
4. **Optional: Set transfer confidence level** - Choose the minimum accuracy for automatic transfer detection:
   - `Exact` (100%) - Only detect transfers with high confidence
   - `Likely` (70%) - Also detect probable transfers (may require review)
5. **Start the import** - The system will:
   - Parse the CSV file using the matching reading settings
   - Register new operations
   - Apply tagging criteria automatically
   - Detect transfers based on your transfer criteria
   - Detect duplicate operations

#### Import Results

After import, you'll receive a summary with:

- **Registered Operations**: New operations successfully imported
- **Duplicates**: Groups of operations that appear to be duplicates (same amount, timestamp, and description)
- **Errors**: Any parsing errors or validation failures
- **Success Messages**: Information about transfers detected, tags applied, etc.

#### Handling Duplicates

The system automatically detects potential duplicates based on:
- Same amount (absolute value)
- Same timestamp (within a small time window)
- Same description

Duplicate groups are shown in the import results. You can:
- Review each duplicate group
- Keep the operations if they're legitimate (not actual duplicates)
- Delete duplicates manually after import

#### Transfer Detection During Import

If transfer detection criteria are configured, the system will automatically:
- Match withdraw operations with income operations
- Tag matched operations as `Transfer`, `Source`, or `Sink`
- Exclude transfers from income/expense calculations

The transfer confidence level you select determines which transfers are automatically detected. You can review and adjust transfers later if needed.

### Operations editing

Edit operations individually or in bulk to correct data, add tags, or update attributes.

#### Editing Individual Operations

1. **Find the operation** in the operations list
2. **Click the edit button** (✏️) to enter edit mode
3. **Modify the fields**:
   - **Description**: Transaction description
   - **Amount**: Transaction amount (positive for income, negative for expenses)
   - **Currency**: Currency code (e.g., USD, EUR, RUB)
   - **Tags**: Add, remove, or modify tags
   - **Attributes**: Add, remove, or modify custom attributes (key-value pairs)
4. **Save changes** (💾) or **Cancel** (✕)

#### Editable Fields

- **Description** (`string`): Free-form text describing the transaction
- **Amount** (`decimal`): Transaction amount with decimal precision
- **Currency Code** (`string`): ISO currency code (3 letters, e.g., USD, EUR)
- **Timestamp** (`DateTime`): Date and time of the transaction
- **Tags** (`string[]`): Array of tag names
- **Attributes** (`Dictionary<string, object>`): Custom key-value pairs for additional metadata

#### Bulk Updates

You can update multiple operations at once by sending a batch update request. The update process will:
- Apply changes to all specified operations
- Re-evaluate tagging criteria (based on tagging mode)
- Re-detect transfers (if transfer confidence level is specified)
- Update operation versions for optimistic concurrency

#### Tagging Modes

When updating operations, you can choose how tags are handled:

- **Append**: Add new tags from tagging criteria without removing existing tags
- **Replace**: Replace all tags with those from tagging criteria
- **None**: Don't apply tagging criteria (keep existing tags)

#### Transfer Re-detection

When updating operations, you can optionally re-run transfer detection:
- Specify a transfer confidence level (`Exact` or `Likely`)
- The system will re-evaluate all operations for potential transfers
- Previously detected transfers may be updated or removed if criteria no longer match

#### Operation Details

Each operation displays:
- **Operation ID**: Unique identifier
- **Budget ID**: The budget this operation belongs to
- **Version**: Version number for optimistic concurrency control

You can expand an operation to view these details.

#### Deleting Operations

Operations can be deleted individually:
1. Click the delete button (🗑️) on the operation
2. Confirm the deletion

**Note**: Deleted operations are permanently removed and cannot be recovered. Consider exporting your data before bulk deletions.

### Logbook