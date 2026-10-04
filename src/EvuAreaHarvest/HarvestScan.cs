using System;
using System.Collections.Generic;
using EvuAreaHarvest.Core;
using UnityEngine;

namespace EvuAreaHarvest;

internal readonly struct HarvestTarget
{
    readonly Func<Player, bool>? _tryHarvest;

    public HarvestTarget(Vector3 position, float distance, bool ready, Func<Player, bool>? tryHarvest)
    {
        Position = position;
        Distance = distance;
        Ready = ready;
        _tryHarvest = tryHarvest;
    }

    public Vector3 Position { get; }

    public float Distance { get; }

    public bool Ready { get; }

    public bool TryHarvest(Player player)
    {
        return _tryHarvest != null && _tryHarvest(player);
    }
}

internal static class HarvestScan
{
    public static List<HarvestTarget> Collect(Vector3 origin)
    {
        var found = new List<HarvestTarget>();
        var seen = new HashSet<int>();

        foreach (var pickable in UnityEngine.Object.FindObjectsByType<Pickable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (pickable == null || pickable.m_itemPrefab == null || !seen.Add(pickable.gameObject.GetInstanceID()))
            {
                continue;
            }

            var position = pickable.transform.position;
            var captured = pickable;
            found.Add(new HarvestTarget(
                position,
                Vector3.Distance(origin, position),
                Ready(captured),
                player => TryPick(captured, player)));
        }

        foreach (var item in UnityEngine.Object.FindObjectsByType<PickableItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (item == null || item.m_itemPrefab == null || !seen.Add(item.gameObject.GetInstanceID()))
            {
                continue;
            }

            var position = item.transform.position;
            var captured = item;
            found.Add(new HarvestTarget(
                position,
                Vector3.Distance(origin, position),
                item.m_nview != null && item.m_nview.IsValid() && !item.m_picked,
                player => TryPickItem(captured, player)));
        }

        foreach (var plant in UnityEngine.Object.FindObjectsByType<Plant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (plant == null || !seen.Add(plant.gameObject.GetInstanceID()))
            {
                continue;
            }

            var position = plant.transform.position;
            found.Add(new HarvestTarget(position, Vector3.Distance(origin, position), false, null));
        }

        return found;
    }

    static bool Ready(Pickable pickable)
    {
        return pickable.m_nview != null && pickable.m_nview.IsValid() && pickable.CanBePicked();
    }

    static bool TryPick(Pickable pickable, Player player)
    {
        if (pickable == null || !Ready(pickable))
        {
            return false;
        }

        var before = pickable.m_pickedLocal;
        pickable.Interact(player, false, false);
        return pickable != null && pickable.m_pickedLocal && !before;
    }

    static bool TryPickItem(PickableItem item, Player player)
    {
        if (item == null || item.m_picked || item.m_nview == null || !item.m_nview.IsValid())
        {
            return false;
        }

        return item.Interact(player, false, false);
    }
}
