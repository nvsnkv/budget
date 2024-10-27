using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Options;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

internal class YamlBasedLogbookCriteriaWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options) : IObjectWriter<LogbookCriteria>
{
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(LowerCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .WithTypeConverter(new CustomLogbookPartsConverter())
        .Build();

    public Task Write(LogbookCriteria criterion, CancellationToken ct) => Write(criterion, options.Value.OutputStreamName, ct);
    public async Task Write(LogbookCriteria criterion, string streamName, CancellationToken ct)
    {
        var stream = await streams.GetOutput(streamName);
        _serializer.Serialize(stream, criterion);
        await stream.FlushAsync(ct);
    }

    public Task Write(IEnumerable<LogbookCriteria> collection, CancellationToken ct) => Write(collection, options.Value.OutputStreamName, ct);
    public async Task Write(IEnumerable<LogbookCriteria> collection, string streamName, CancellationToken ct)
    {
        foreach (var criteria in collection)
        {
            await Write(criteria, streamName, ct);
        }
    }

    private class CustomLogbookPartsConverter : IYamlTypeConverter
    {
        private static readonly Type[] SupportedTypes =
        [
            typeof(LogbookCriteria)
        ];

        public bool Accepts(Type type) => SupportedTypes.Contains(type);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is LogbookCriteria criteria)
            {
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
                if (criteria.Criteria != null)
                {
                    emitter.Emit(new Scalar(null, "criteria"));
                    emitter.Emit(new Scalar(null, criteria.Criteria.ToString()));
                }

                if (criteria.Substitution != null)
                {
                    emitter.Emit(new Scalar(null, "substitution"));
                    emitter.Emit(new Scalar(null, criteria.Substitution.ToString()));
                }

                if (criteria.Tags != null)
                {
                    emitter.Emit(new Scalar(null, "tags"));
                    emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                    foreach (var tag in criteria.Tags)
                    {
                        emitter.Emit(new Scalar(tag.Value));
                    }

                    emitter.Emit(new SequenceEnd());
                }

                if (criteria.Type != null)
                {
                    emitter.Emit(new Scalar(null, "type"));
                    emitter.Emit(new Scalar(criteria.Type.ToString()!));
                }

                var subcriteria = criteria.Subcriteria;
                if (subcriteria != null && subcriteria.Any())
                {
                    emitter.Emit(new Scalar(null, "subcriteria"));
                    emitter.Emit(new MappingStart());

                    foreach (var subcriterion in subcriteria)
                    {
                        emitter.Emit(new Scalar(null, subcriterion.Description));
                        serializer(subcriterion);
                    }

                    emitter.Emit(new MappingEnd());
                }

                emitter.Emit(new MappingEnd());
            }
        }
    }
}
