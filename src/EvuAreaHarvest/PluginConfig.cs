using BepInEx.Configuration;
using EvuAreaHarvest.Core;
using UnityEngine;
// ConfigurationManagerAttributes is shipped by Jotunn.dll in the global namespace. Jotunn reads IsAdminOnly
// from it to decide which entries sync from the server, and Configuration Manager reads Order and the S button.
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
                "Radius in meters, from 2 to 32. Harvest and the highlight use a sphere centered on you. When this mod is installed on the server, that server's value is the one used.",
                new AcceptableValueRange<float>(HarvestMath.MinRange, HarvestMath.MaxRange),
                ConfigHints.Synced(100)));
        PickGuarded = config.Bind(
            "Server",
            "Pick guarded items",
            false,
            new ConfigDescription(
                "When on, also picks items that aggravate nearby creatures when taken, such as Dvergr belongings. Off leaves them alone.",
                null,
                ConfigHints.Synced(95)));
        HighlightHarvestables = config.Bind(
            "Local",
            "Highlight harvestables",
            false,
            new ConfigDescription(
                "When on, draws the range on the ground and a marker on each harvestable in range.",
                null,
                ConfigHints.Local(90)));
        HarvestArea = config.Bind(
            "Local",
            "Harvest area",
            new KeyboardShortcut(KeyCode.KeypadPeriod),
            new ConfigDescription(
                "Key that harvests everything in range. Unity calls numpad Delete KeypadPeriod.",
                null,
                ConfigHints.Local(80)));
        ToggleHighlight = config.Bind(
            "Local",
            "Toggle highlight",
            new KeyboardShortcut(KeyCode.End),
            new ConfigDescription(
                "Key that shows or hides the range ring and the harvestable markers.",
                null,
                ConfigHints.Local(70)));
    }

    public ConfigEntry<float> HarvestRange { get; }

    public ConfigEntry<bool> PickGuarded { get; }

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
