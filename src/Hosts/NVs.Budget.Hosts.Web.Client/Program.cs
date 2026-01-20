var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseDefaultFiles().UseStaticFiles();
app.MapGet("/health", () => Results.Ok());

// Runtime configuration endpoint for Angular app
app.MapGet("/api/config", (IConfiguration configuration) =>
{
    var apiUrl = configuration["ApiUrl"] ?? "https://localhost:25001";
    return Results.Ok(new { apiUrl });
});

app.Run();
