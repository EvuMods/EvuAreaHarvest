using BepInEx;
using EvuAreaHarvest.Core;
using Jotunn.Utils;
using UnityEngine;

namespace EvuAreaHarvest;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
[SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
public sealed class Plugin : BaseUnityPlugin
{
    internal static PluginConfig Settings { get; private set; } = null!;

    internal static BepInEx.Logging.ManualLogSource Log { get; private set; } = null!;

    ConfigFileWatcher _configWatcher = null!;
    HarvestHighlight _highlight = null!;

    void Awake()
    {
        Log = Logger;
        Settings = new PluginConfig(Config);
        _configWatcher = new ConfigFileWatcher(Config);
        _highlight = new HarvestHighlight(transform);
        Logger.LogInfo("EvuAreaHarvest loaded.");
    }

    void OnDestroy()
    {
        if (_highlight != null)
        {
            _highlight.Destroy();
        }
    }

    void Update()
    {
        var player = Player.m_localPlayer;
        if (player == null)
        {
            _highlight.Tick(null!, false, 0f, false);
            return;
        }

        if (AcceptsHotkeys(player))
        {
            if (Settings.HarvestArea.Value.IsDown())
            {
                HarvestNow(player);
            }

            if (Settings.ToggleHighlight.Value.IsDown())
            {
                Settings.HighlightHarvestables.Value = !Settings.HighlightHarvestables.Value;
                Say(player, HarvestMath.FormatHighlight(Settings.HighlightHarvestables.Value));
            }
        }

        _highlight.Tick(player, Settings.HighlightHarvestables.Value, HarvestMath.ClampRange(Settings.HarvestRange.Value), Settings.PickGuarded.Value);
    }

    static void HarvestNow(Player player)
    {
        var range = HarvestMath.ClampRange(Settings.HarvestRange.Value);
        var found = HarvestScan.Collect(player.transform.position, range, Settings.PickGuarded.Value, out _);
        var candidates = new HarvestTally.Candidate[found.Count];
        for (var i = 0; i < found.Count; i++)
        {
            candidates[i] = new HarvestTally.Candidate(found[i].Distance, found[i].Ready);
        }

        var result = HarvestTally.Run(candidates, range, index => found[index].TryHarvest(player));
        Say(player, HarvestMath.FormatHarvest(result.Harvested, result.InRange));
    }

    static void Say(Player player, string line)
    {
        Log.LogInfo(line);
        player.Message(MessageHud.MessageType.Center, line, 0, null, false);
    }

    /// <summary>
    /// Same gate Valheim uses for its own keys: no menu, map, inventory, store, console, chat, or text box open,
    /// and the player is in a state where a hand pick would work.
    /// </summary>
    static bool AcceptsHotkeys(Player player)
    {
        return player.TakeInput() && !player.IsDead() && !player.InCutscene() && !player.IsTeleporting();
    }
}
