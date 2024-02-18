using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class TransferDetectorShould
{
    private readonly Fixture _fixture = new();
    [Fact]
    public void DetectTransfersUsingCriteriaList()
    {
        _fixture.Customizations.Add(new NamedParameterBuilder<CurrencyIsoCode>("currency", _fixture.Create<CurrencyIsoCode>(), false));
        var generator = new RandomNumericSequenceGenerator(-100, -1);
        _fixture.Customizations.Add(generator);
        var source = _fixture.Create<TrackedTransaction>();

        _fixture.Customizations.Remove(generator);
        _fixture.Customizations.Add(new RandomNumericSequenceGenerator(1, 100));

        var sink = _fixture.Create<TrackedTransaction>();
        var maybeSink = _fixture.Create<TrackedTransaction>();

        var criteria = new TransferCriterion[]
        {
            new(DetectionAccuracy.Exact, "Exactly!", (src, snk) => src == source && snk == sink),
            new(DetectionAccuracy.Likely, "Maybe", (src, _) => src == source)
        };

        var detector = new TransferDetector(criteria);
        var result = detector.Detect(source, sink);
        result.IsSuccess.Should().BeTrue();
        result.Value.Accuracy.Should().Be(criteria[0].Accuracy);
        result.Value.Source.Should().Be(source);
        result.Value.Sink.Should().Be(sink);
        result.Value.Comment.Should().Be(criteria[0].Comment);

        result = detector.Detect(source, maybeSink);
        result.IsSuccess.Should().BeTrue();
        result.Value.Accuracy.Should().Be(criteria[1].Accuracy);
        result.Value.Source.Should().Be(source);
        result.Value.Sink.Should().Be(maybeSink);
        result.Value.Comment.Should().Be(criteria[1].Comment);

        result = detector.Detect(maybeSink, sink);
        result.IsSuccess.Should().BeFalse();
    }
}
