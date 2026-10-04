using BepInEx.Configuration;
using EvuAreaHarvest.Core;
using UnityEngine;
using SyncHint = global::ConfigurationManagerAttributes;

namespace EvuAreaHarvest;

internal sealed class PluginConfig
{
    public PluginConfig(ConfigFile config)
    {
        HarvestRange = config.Bind(
            "Server",
            "Harvest range",
            HarvestMath.DefaultRange,
            new ConfigDescription(
                "Radius in meters. Harvest and the highlight use a sphere centered on you. When this mod is installed on the server, that server's value is the one used.",
                new AcceptableValueRange<float>(HarvestMath.MinRange, HarvestMath.MaxRange),
                ConfigHints.Synced(100)));
        HighlightHarvestables = config.Bind(
            "Local",
            "Highlight harvestables",
            false,
            new ConfigDescription(
                "When on, draws the range on the ground and a marker on each harvestable in range. This client only.",
                null,
                ConfigHints.Local(90)));
        HarvestArea = config.Bind(
            "Local",
            "Harvest area",
            new KeyboardShortcut(KeyCode.KeypadPeriod),
            new ConfigDescription(
                "Key that harvests everything in range. Default is numpad Delete.",
                null,
                ConfigHints.Local(80)));
        ToggleHighlight = config.Bind(
            "Local",
            "Toggle highlight",
            new KeyboardShortcut(KeyCode.End),
            new ConfigDescription(
                "Key that shows or hides the range ring and the harvestable markers. Default is End. This client only.",
                null,
                ConfigHints.Local(70)));
    }

    public ConfigEntry<float> HarvestRange { get; }

    public ConfigEntry<bool> HighlightHarvestables { get; }

    public ConfigEntry<KeyboardShortcut> HarvestArea { get; }

    public ConfigEntry<KeyboardShortcut> ToggleHighlight { get; }
}

static class ConfigHints
{
    public static SyncHint Synced(int order)
    {
        return new SyncHint { IsAdminOnly = true, Order = order };
    }

    public static SyncHint Local(int order)
    {
        return new SyncHint { IsAdminOnly = false, Order = order };
    }
}
