using System.Collections.Generic;
using EvuAreaHarvest.Core;
using Xunit;

namespace EvuAreaHarvest.Core.Tests;

public sealed class HarvestTallyTests
{
    [Fact]
    public void ReadySubset_CountsTheRestAsInRange()
    {
        var calls = new List<int>();
        var candidates = new[]
        {
            new HarvestTally.Candidate(5f, true),
            new HarvestTally.Candidate(1f, false),
            new HarvestTally.Candidate(3f, true),
            new HarvestTally.Candidate(40f, true),
        };

        var result = HarvestTally.Run(candidates, 8f, index =>
        {
            calls.Add(index);
            return true;
        });

        Assert.Equal(2, result.Harvested);
        Assert.Equal(3, result.InRange);
        Assert.Equal(new[] { 2, 0 }, calls);
    }

    [Fact]
    public void Empty_ReportsZeroOutOfZero()
    {
        var result = HarvestTally.Run(new HarvestTally.Candidate[0], 8f, _ => true);

        Assert.Equal(0, result.Harvested);
        Assert.Equal(0, result.InRange);
        Assert.Equal("Harvested 0 out of 0 in range", HarvestMath.FormatHarvest(result.Harvested, result.InRange));
    }

    [Fact]
    public void NotReady_IsCountedAndNotHarvested()
    {
        var called = false;
        var result = HarvestTally.Run(
            new[] { new HarvestTally.Candidate(2f, false) },
            8f,
            _ =>
            {
                called = true;
                return true;
            });

        Assert.False(called);
        Assert.Equal(0, result.Harvested);
        Assert.Equal(1, result.InRange);
    }

    [Fact]
    public void FailedHarvest_StaysInTheTotal()
    {
        var result = HarvestTally.Run(
            new[] { new HarvestTally.Candidate(1f, true) },
            8f,
            _ => false);

        Assert.Equal(0, result.Harvested);
        Assert.Equal(1, result.InRange);
    }

    [Fact]
    public void EdgeOfTheSphere_IsInside()
    {
        Assert.True(HarvestMath.InRange(8f, 8f));
        Assert.False(HarvestMath.InRange(8.01f, 8f));

        var result = HarvestTally.Run(
            new[] { new HarvestTally.Candidate(8f, true) },
            8f,
            _ => true);

        Assert.Equal(1, result.Harvested);
        Assert.Equal(1, result.InRange);
    }

    [Fact]
    public void Range_ClampsToTheSlider()
    {
        Assert.Equal(2f, HarvestMath.ClampRange(0f));
        Assert.Equal(2f, HarvestMath.ClampRange(2f));
        Assert.Equal(8f, HarvestMath.ClampRange(HarvestMath.DefaultRange));
        Assert.Equal(32f, HarvestMath.ClampRange(32f));
        Assert.Equal(32f, HarvestMath.ClampRange(100f));
    }

    [Fact]
    public void Range_NaNFallsBackToTheDefault()
    {
        Assert.Equal(HarvestMath.DefaultRange, HarvestMath.ClampRange(float.NaN));
        Assert.Equal(32f, HarvestMath.ClampRange(float.PositiveInfinity));
        Assert.Equal(2f, HarvestMath.ClampRange(float.NegativeInfinity));
    }

    [Fact]
    public void Messages_MatchTheLogLines()
    {
        Assert.Equal("Harvested 3 out of 10 in range", HarvestMath.FormatHarvest(3, 10));
        Assert.Equal("Highlight harvestables on", HarvestMath.FormatHighlight(true));
        Assert.Equal("Highlight harvestables off", HarvestMath.FormatHighlight(false));
    }
}
