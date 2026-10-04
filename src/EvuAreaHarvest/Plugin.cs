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
        if (Settings == null || player == null)
        {
            if (_highlight != null)
            {
                _highlight.Hide();
            }

            return;
        }

        if (!Typing())
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

        _highlight.Tick(player, Settings.HighlightHarvestables.Value, HarvestMath.ClampRange(Settings.HarvestRange.Value));
    }

    static void HarvestNow(Player player)
    {
        var range = HarvestMath.ClampRange(Settings.HarvestRange.Value);
        var found = HarvestScan.Collect(player.transform.position);
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

    static bool Typing()
    {
        if (Console.instance != null && Console.IsVisible())
        {
            return true;
        }

        if (Chat.instance != null && Chat.instance.HasFocus())
        {
            return true;
        }

        return TextInput.instance != null && TextInput.IsVisible();
    }
}
