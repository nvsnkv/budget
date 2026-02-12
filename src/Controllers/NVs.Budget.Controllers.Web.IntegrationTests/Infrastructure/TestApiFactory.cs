using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Controllers.Web;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.IntegrationTests.Infrastructure;

internal sealed class TestApiFactory : IAsyncDisposable
{
    private readonly WebApplication _app;
    public HttpClient Client { get; }

    public TestApiFactory(IEnumerable<TrackedBudget> budgets)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("TestAuth")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<IMediator>(_ => new InMemoryMediator(budgets));
        builder.Services.AddSingleton(ReadableExpressionsParser.Default);
        builder.Services.AddSingleton<ICsvFileReader, StubCsvFileReader>();
        builder.Services.AddSingleton<IReadingSettingsRepository, StubReadingSettingsRepository>();
        builder.Services.AddWebControllers();

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.StartAsync().GetAwaiter().GetResult();
        Client = _app.GetTestClient();
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return _app.DisposeAsync();
    }
}
