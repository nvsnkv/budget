using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using YamlDotNet.Serialization;

namespace NVs.Budget.Controllers.Web.Formatters;

internal class YamlOutputFormatter : TextOutputFormatter
{
    private readonly ISerializer _serializer;

    public YamlOutputFormatter(ISerializer serializer)
    {
        _serializer = serializer;
        
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
        SupportedMediaTypes.Add("application/yaml");
        SupportedMediaTypes.Add("text/yaml");
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;
        using (var writer = context.WriterFactory(response.Body, selectedEncoding))
        {
            _serializer.Serialize(writer, context.Object);
            await writer.FlushAsync();
        }
    }
}
