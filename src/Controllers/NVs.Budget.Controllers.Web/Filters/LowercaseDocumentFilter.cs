using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NVs.Budget.Controllers.Web.Filters;

internal class LowercaseDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.ToDictionary(
            entry => ToLowercasePath(entry.Key),
            entry => entry.Value
        );
    
        swaggerDoc.Paths.Clear();
        foreach (var (key, value) in paths)
        {
            swaggerDoc.Paths.Add(key, value);
        }
    }

    private static string ToLowercasePath(string path)
    {
        // Convert controller names to lowercase while preserving the rest of the path
        return System.Text.RegularExpressions.Regex.Replace(
            path, 
            @"/([A-Z][a-z]*)(?=/|$)", 
            match => "/" + match.Groups[1].Value.ToLowerInvariant()
        );
    }
}
