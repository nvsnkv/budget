﻿using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Transfers;

namespace NVs.Budget.Application.Tests;

public class TransferDetectorShould
{
    private readonly Fixture _fixture = new();
    [Fact]
    public void DetectTransfersUsingCriteriaList()
    {
        using var _ = _fixture.SetCurrency(_fixture.Create<CurrencyIsoCode>());
        var source = _fixture.CreateWithdraws<TrackedTransaction>(1).Single();
        var sink = _fixture.CreateIncomes<TrackedTransaction>(1).Single();
        var maybeSink = _fixture.CreateIncomes<TrackedTransaction>(1).Single();

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

    [Theory]
    [MemberData(nameof(GetBadOptions))]
    public void PreserveDomainInvariants(TrackedTransaction source, TrackedTransaction sink, string error)
    {
        var criteria = new TransferCriterion[] { new(DetectionAccuracy.Exact, "For sure!", (_ , _) => true) };
        var detector = new TransferDetector(criteria);

        detector.Detect(source, sink).IsSuccess.Should().BeFalse($"Following error was not tracked: {error}");
    }

    public static IEnumerable<object[]> GetBadOptions()
    {
        var fixture = new Fixture();
        using (fixture.SetCurrency(fixture.Create<CurrencyIsoCode>()))
        {
            var withdraws = fixture.CreateWithdraws<TrackedTransaction>(2);
            var incomes = fixture.CreateIncomes<TrackedTransaction>(2);

            yield return [withdraws[0], withdraws[1], "both are withdraws"];
            yield return [incomes[0], withdraws[1], "reversed params, incomes passed instead of withdraw"];
            yield return [incomes[0], incomes[1], "both are incomes"];
        }

        fixture.SetCurrency(fixture.Create<CurrencyIsoCode>());
        var source = fixture.CreateWithdraws<TrackedTransaction>(1).Single();
        fixture.SetCurrency(fixture.Create<CurrencyIsoCode>());
        var sink = fixture.CreateIncomes<TrackedTransaction>(1).Single();

        yield return [source, sink, "currencies are different"];
    }
}
