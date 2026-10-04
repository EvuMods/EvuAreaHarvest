namespace EvuAreaHarvest.Core;

public static class HarvestMath
{
    public const float MinRange = 2f;

    public const float MaxRange = 32f;

    public const float DefaultRange = 8f;

    public static float ClampRange(float range)
    {
        if (range < MinRange)
        {
            return MinRange;
        }

        if (range > MaxRange)
        {
            return MaxRange;
        }

        return range;
    }

    public static bool InRange(float distance, float range)
    {
        return distance <= range;
    }

    public static string FormatHarvest(int harvested, int inRange)
    {
        return "Harvested " + harvested + " out of " + inRange + " in range";
    }

    public static string FormatHighlight(bool on)
    {
        return on ? "Highlight harvestables on" : "Highlight harvestables off";
    }
}
