using System;
using System.Collections.Generic;

namespace EvuAreaHarvest.Core;

public static class HarvestTally
{
    public readonly struct Candidate
    {
        public Candidate(float distance, bool ready)
        {
            Distance = distance;
            Ready = ready;
        }

        public float Distance { get; }

        public bool Ready { get; }
    }

    public readonly struct Result
    {
        public Result(int harvested, int inRange)
        {
            Harvested = harvested;
            InRange = inRange;
        }

        public int Harvested { get; }

        public int InRange { get; }
    }

    public static Result Run(IReadOnlyList<Candidate> candidates, float range, Func<int, bool> tryHarvest)
    {
        var order = new int[candidates.Count];
        for (var i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        Array.Sort(order, (a, b) => candidates[a].Distance.CompareTo(candidates[b].Distance));

        var harvested = 0;
        var inRange = 0;
        foreach (var index in order)
        {
            var candidate = candidates[index];
            if (!HarvestMath.InRange(candidate.Distance, range))
            {
                break;
            }

            inRange++;
            if (candidate.Ready && tryHarvest(index))
            {
                harvested++;
            }
        }

        return new Result(harvested, inRange);
    }
}
