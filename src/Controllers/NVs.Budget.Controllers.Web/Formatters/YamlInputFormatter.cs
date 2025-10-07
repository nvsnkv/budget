using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using YamlDotNet.Serialization;

namespace NVs.Budget.Controllers.Web.Formatters;

internal class YamlInputFormatter : TextInputFormatter
{
    private readonly IDeserializer _deserializer;

    public YamlInputFormatter(IDeserializer deserializer)
    {
        _deserializer = deserializer;
        
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
        SupportedMediaTypes.Add("application/yaml");
        SupportedMediaTypes.Add("text/yaml");
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var httpContext = context.HttpContext;
        
        using (var reader = new StreamReader(httpContext.Request.Body, encoding))
        {
            try
            {
                var yaml = await reader.ReadToEndAsync();
                var model = _deserializer.Deserialize(yaml, context.ModelType);
                return await InputFormatterResult.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError(string.Empty, $"Failed to parse YAML: {ex.Message}");
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}

