using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Options;
using YamlDotNet.Serialization;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

internal class YamlBasedTaggingCriteriaWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IObjectWriter<TaggingCriterion>
{
    private readonly ISerializer _serializer = new SerializerBuilder().Build();

    public Task Write(TaggingCriterion criterion, CancellationToken ct) => Write(criterion, options.Value.OutputStreamName, ct);
    public Task Write(TaggingCriterion criterion, string steamName, CancellationToken ct) => Write(Enumerable.Repeat(criterion, 1), steamName, ct);

    public Task Write(IEnumerable<TaggingCriterion> collection, CancellationToken ct) => Write(collection, options.Value.OutputStreamName, ct);
    public async Task Write(IEnumerable<TaggingCriterion> collection, string streamName, CancellationToken ct)
    {
        var dict = collection
            .GroupBy(t => t.Tag.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Select(t => t.Condition.ToString()).ToList()
            );

        var stream = await streams.GetOutput(streamName);

        _serializer.Serialize(stream, dict);
        await stream.FlushAsync(ct);
    }
}
