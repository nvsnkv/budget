using System.Globalization;
using System.Text;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;

namespace NVs.Budget.Infrastructure.Files.CSV.Tests;

public class CsvFileReaderShould
{
    private const string DefaultTimeZone = "UTC";

    private readonly CsvFileReader _reader;

    public CsvFileReaderShould()
    {
        _reader = new CsvFileReader();
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    [Fact]
    public async Task ReadSimpleOperationsSuccessfully()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD,Coffee shop
            2024-01-16,200.00,USD,Grocery store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(100.50m, Currency.Usd));
        results[0].Value.Description.Should().Be("Coffee shop");
        
        results[1].Value.Timestamp.Should().Be(new DateTime(2024, 1, 16, 0, 0, 0, DateTimeKind.Utc));
        results[1].Value.Amount.Should().Be(new Money(200.00m, Currency.Usd));
        results[1].Value.Description.Should().Be("Grocery store");
    }

    [Fact]
    public async Task ReadOperationsWithPatternCombinations()
    {
        // Arrange
        var csv = """
            2024-01-15,100,50,USD,Coffee,shop
            2024-01-16,200,00,EUR,Grocery,store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}.{2}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{3}",
                [nameof(UnregisteredOperation.Description)] = "{4} {5}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        
        results[0].Value.Amount.Should().Be(new Money(100.50m, Currency.Usd));
        results[0].Value.Description.Should().Be("Coffee shop");
        
        results[1].Value.Amount.Should().Be(new Money(200.00m, Currency.Eur));
        results[1].Value.Description.Should().Be("Grocery store");
    }

    [Fact]
    public async Task ReadOperationsWithAttributes()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD,Coffee shop,Card,Personal
            2024-01-16,200.00,USD,Grocery store,Cash,Business
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>
            {
                ["PaymentMethod"] = "{4}",
                ["Category"] = "{5}"
            },
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        
        results[0].Value.Attributes.Should().NotBeNull();
        results[0].Value.Attributes!["PaymentMethod"].Should().Be("Card");
        results[0].Value.Attributes!["Category"].Should().Be("Personal");
        
        results[1].Value.Attributes.Should().NotBeNull();
        results[1].Value.Attributes!["PaymentMethod"].Should().Be("Cash");
        results[1].Value.Attributes!["Category"].Should().Be("Business");
    }

    [Fact]
    public async Task SkipRowsThatFailValidation_Equals()
    {
        // Arrange
        var csv = """
            header,row,to,skip
            2024-01-15,100.50,USD,Coffee shop
            2024-01-16,200.00,USD,Grocery store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: new[]
            {
                new ValidationRule(
                    Pattern: "{0}",
                    Condition: ValidationRule.ValidationCondition.NotEquals,
                    Value: "header",
                    ErrorMessage: ""
                )
            }
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
    }

    [Fact]
    public async Task ReturnFailureForRowsWithValidationError()
    {
        // Arrange
        var csv = """
            invalid,100.50,USD,Coffee shop
            2024-01-16,200.00,USD,Grocery store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: new[]
            {
                new ValidationRule(
                    Pattern: "{0}",
                    Condition: ValidationRule.ValidationCondition.NotEquals,
                    Value: "invalid",
                    ErrorMessage: "Invalid row detected"
                )
            }
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().BeFailure();
        results[0].Errors.Should().ContainSingle(e => e.Message == "Unable to parse row!");
        
        results[1].Should().BeSuccess();
    }

    [Fact]
    public async Task ReturnFailureForInvalidDateFormat()
    {
        // Arrange
        var csv = """
            not-a-date,100.50,USD,Coffee shop
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().ContainSingle();
        results[0].Should().BeFailure();
        results[0].Errors.Should().ContainSingle(e => e.Message == "Unable to parse row!");
    }

    [Fact]
    public async Task ReturnFailureForInvalidAmountFormat()
    {
        // Arrange
        var csv = """
            2024-01-15,not-a-number,USD,Coffee shop
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().ContainSingle();
        results[0].Should().BeFailure();
        results[0].Errors.Should().ContainSingle(e => e.Message == "Unable to parse row!");
    }

    [Fact]
    public async Task ReturnFailureForInvalidCurrencyCode()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,INVALID,Coffee shop
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().ContainSingle();
        results[0].Should().BeFailure();
        results[0].Errors.Should().ContainSingle(e => e.Message == "Unable to parse row!");
    }

    [Fact]
    public async Task ReturnFailureForMissingRequiredField()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD,Coffee shop
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                // Missing Timestamp field
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().ContainSingle();
        results[0].Should().BeFailure();
        results[0].Errors[0].Reasons.Should().ContainSingle(e => e.Message.Contains("No field options provided"));
    }

    [Fact]
    public async Task HandleEmptyRows()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD,Coffee shop

            2024-01-16,200.00,USD,Grocery store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
    }

    [Fact]
    public async Task ReadOperationsWithLocalDateTimeKind_ConvertsUsingClientTimeZone()
    {
        const string clientTimeZone = "Europe/Berlin";
        var csv = """
            2024-01-15 14:30:00,1234.56,EUR,Coffee shop
            """;
        var stream = CreateStreamReader(csv);

        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Local,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, clientTimeZone, CancellationToken.None));

        results.Should().ContainSingle();
        results[0].Should().BeSuccess();
        results[0].Value.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(
            results[0].Value.Timestamp,
            TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone));
        local.Hour.Should().Be(14);
        local.Minute.Should().Be(30);
        results[0].Value.Amount.Should().Be(new Money(1234.56m, Currency.Eur));
    }

    [Fact]
    public async Task HandleComplexValidationRules()
    {
        // Arrange
        var csv = """
            expense,2024-01-15,100.50,USD,Coffee shop
            income,2024-01-16,500.00,USD,Salary
            expense,2024-01-17,200.00,USD,Grocery store
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{1}",
                [nameof(UnregisteredOperation.Amount)] = "{2}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{3}",
                [nameof(UnregisteredOperation.Description)] = "{4}"
            },
            Attributes: new Dictionary<string, string>
            {
                ["Type"] = "{0}"
            },
            Validation: new[]
            {
                new ValidationRule(
                    Pattern: "{0}",
                    Condition: ValidationRule.ValidationCondition.Equals,
                    Value: "expense",
                    ErrorMessage: ""
                )
            }
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2); // Only expense rows should be processed
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Description.Should().Be("Coffee shop");
        results[1].Value.Description.Should().Be("Grocery store");
    }

    [Fact]
    public async Task IncludeRowNumberInErrorMetadata()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD,Coffee shop
            invalid-date,200.00,USD,Grocery store
            2024-01-17,300.00,USD,Restaurant
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(3);
        results[0].Should().BeSuccess();
        results[1].Should().BeFailure();
        results[1].Errors[0].Metadata.Should().ContainKey("row");
        results[1].Errors[0].Metadata["row"].Should().Be(2);
        results[2].Should().BeSuccess();
    }

    [Fact]
    public async Task HandlePatternWithoutPlaceholders()
    {
        // Arrange
        var csv = """
            2024-01-15,100.50,USD
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "Default description" // No placeholder
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().ContainSingle();
        results[0].Should().BeSuccess();
        results[0].Value.Description.Should().Be("Default description");
    }

    [Fact]
    public async Task ReadOperationsWithFrenchCulture()
    {
        // Arrange - French culture (fr-FR): uses semicolon delimiter, comma as decimal separator
        var csv = """
            2024-01-15;1234,56;EUR;Café
            2024-01-16;200,00;EUR;Résumé financier
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: new CultureInfo("fr-FR"),
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(1234.56m, Currency.Eur));
        results[0].Value.Description.Should().Be("Café");
        results[1].Value.Amount.Should().Be(new Money(200.00m, Currency.Eur));
        results[1].Value.Description.Should().Be("Résumé financier");
    }

    [Fact]
    public async Task ReadOperationsWithRussianCulture()
    {
        // Arrange - Russian culture (ru-RU): uses semicolon delimiter, comma as decimal separator
        var csv = """
            2024-01-15;100,50;EUR;Магазин
            2024-01-16;250,75;EUR;Ресторан
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: new CultureInfo("ru-RU"),
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(100.50m, Currency.Eur));
        results[0].Value.Description.Should().Be("Магазин");
        results[1].Value.Amount.Should().Be(new Money(250.75m, Currency.Eur));
        results[1].Value.Description.Should().Be("Ресторан");
    }

    [Fact]
    public async Task ReadOperationsWithJapaneseCulture()
    {
        // Arrange - Japanese culture (ja-JP): uses comma delimiter, period as decimal separator
        var csv = """
            2024-01-15,1000.00,JPY,コーヒー
            2024-01-16,5000.50,JPY,レストラン
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: new CultureInfo("ja-JP"),
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(1000m, Currency.Jpy));
        results[0].Value.Description.Should().Be("コーヒー");
        results[1].Value.Amount.Should().Be(new Money(5000.50m, Currency.Jpy));
        results[1].Value.Description.Should().Be("レストラン");
    }

    [Fact]
    public async Task ReadOperationsWithUTF16Encoding()
    {
        // Arrange - UTF-16 encoded file
        var csv = """
            2024-01-15,100.50,USD,Coffee shop
            2024-01-16,200.00,USD,Store
            """;
        var stream = CreateStreamReader(csv, Encoding.Unicode);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.Unicode,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
    }

    [Fact]
    public async Task ReadOperationsWithUTF32Encoding()
    {
        // Arrange - UTF-32 encoded file with emoji
        var csv = """
            2024-01-15,100.50,USD,Coffee ☕
            2024-01-16,200.00,USD,Restaurant 🍽️
            """;
        var stream = CreateStreamReader(csv, Encoding.UTF32);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: Encoding.UTF32,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Description.Should().Be("Coffee ☕");
        results[1].Value.Description.Should().Be("Restaurant 🍽️");
    }

    [Fact]
    public async Task ReadOperationsWithLatin1Encoding()
    {
        // Arrange - Latin1 (ISO-8859-1) encoding with special characters
        var csv = """
            2024-01-15,100.50,EUR,Café résumé
            2024-01-16,200.00,EUR,Naïve émigré
            """;
        var encoding = Encoding.GetEncoding("ISO-8859-1");
        var stream = CreateStreamReader(csv, encoding);
        
        var config = new FileReadingSetting(
            Culture: CultureInfo.InvariantCulture,
            Encoding: encoding,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Description.Should().Be("Café résumé");
        results[1].Value.Description.Should().Be("Naïve émigré");
    }

    [Fact]
    public async Task ReadOperationsWithChineseCulture()
    {
        // Arrange - Chinese culture (zh-CN): uses comma delimiter, period as decimal separator
        var csv = """
            2024-01-15,1234.56,CNY,咖啡店
            2024-01-16,200.00,CNY,超市
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: new CultureInfo("zh-CN"),
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(1234.56m, Currency.Cny));
        results[0].Value.Description.Should().Be("咖啡店");
        results[1].Value.Amount.Should().Be(new Money(200.00m, Currency.Cny));
        results[1].Value.Description.Should().Be("超市");
    }

    [Fact]
    public async Task ReadOperationsWithArabicCulture()
    {
        // Arrange - English (US) culture with Arabic text (demonstrating RTL text support)
        var csv = """
            2024-01-15,1234.50,USD,مقهى
            2024-01-16,200.00,USD,مطعم
            """;
        var stream = CreateStreamReader(csv);
        
        var config = new FileReadingSetting(
            Culture: new CultureInfo("en-US"),
            Encoding: Encoding.UTF8,
            DateTimeKind: DateTimeKind.Utc,
            Fields: new Dictionary<string, string>
            {
                [nameof(UnregisteredOperation.Timestamp)] = "{0}",
                [nameof(UnregisteredOperation.Amount)] = "{1}",
                [nameof(UnregisteredOperation.Amount.CurrencyCode)] = "{2}",
                [nameof(UnregisteredOperation.Description)] = "{3}"
            },
            Attributes: new Dictionary<string, string>(),
            Validation: Array.Empty<ValidationRule>()
        );

        // Act
        var results = await ToListAsync(_reader.ReadUntrackedOperations(stream, config, DefaultTimeZone, CancellationToken.None));

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Should().BeSuccess());
        results[0].Value.Timestamp.Should().Be(new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        results[0].Value.Amount.Should().Be(new Money(1234.50m, Currency.Usd));
        results[0].Value.Description.Should().Be("مقهى");
        results[1].Value.Amount.Should().Be(new Money(200.00m, Currency.Usd));
        results[1].Value.Description.Should().Be("مطعم");
    }

    private static StreamReader CreateStreamReader(string content, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new StreamReader(stream, encoding);
    }
}
