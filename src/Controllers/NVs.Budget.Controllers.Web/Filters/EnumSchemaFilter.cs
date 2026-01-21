using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NVs.Budget.Controllers.Web.Filters;

internal class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            
            // Get the enum values as strings
            var enumValues = Enum.GetNames(context.Type);
            foreach (var enumValue in enumValues)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue));
            }
            
            // Set the type to string since we're using string values
            schema.Type = "string";
        }
        else if (context.Type.IsGenericType)
        {
            // Handle nullable enums
            var underlyingType = Nullable.GetUnderlyingType(context.Type);
            if (underlyingType?.IsEnum == true)
            {
                schema.Enum.Clear();
                
                var enumValues = Enum.GetNames(underlyingType);
                foreach (var enumValue in enumValues)
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue));
                }
                
                schema.Type = "string";
            }
        }
    }
}
