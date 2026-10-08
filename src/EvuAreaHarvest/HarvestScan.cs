using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Whether a grown prefab of a <see cref="Plant"/> turns into something this mod can pick.
    /// Tree saplings grow into trees, so they are not harvestables waiting.
    /// </summary>
    static readonly Dictionary<GameObject, bool> GrowsIntoPickable = new Dictionary<GameObject, bool>();

    /// <summary>
    /// Everything inside <paramref name="range"/> of <paramref name="origin"/> that is, or will become, a pickable.
    /// Objects without a live ZNetView (placement ghosts, prefabs that are not in the world) are skipped.
    /// </summary>
    public static List<HarvestTarget> Collect(Vector3 origin, float range, bool pickGuarded)
    {
        var found = new List<HarvestTarget>();
        var rangeSqr = range * range;

        foreach (var pickable in UnityEngine.Object.FindObjectsByType<Pickable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (pickable == null || pickable.m_itemPrefab == null || !Live(pickable.m_nview) || pickable.m_enabled == 0)
            {
                continue;
            }

            if (!pickGuarded && pickable.m_aggravateRange > 0f)
            {
                continue;
            }

            var position = pickable.transform.position;
            var offset = position - origin;
            if (offset.sqrMagnitude > rangeSqr)
            {
                continue;
            }

            var captured = pickable;
            found.Add(new HarvestTarget(
                position,
                offset.magnitude,
                Ready(captured),
                player => TryPick(captured, player)));
        }

        foreach (var item in UnityEngine.Object.FindObjectsByType<PickableItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (item == null || item.m_itemPrefab == null || !Live(item.m_nview))
            {
                continue;
            }

            var position = item.transform.position;
            var offset = position - origin;
            if (offset.sqrMagnitude > rangeSqr)
            {
                continue;
            }

            var captured = item;
            found.Add(new HarvestTarget(
                position,
                offset.magnitude,
                !item.m_picked,
                player => TryPickItem(captured, player)));
        }

        foreach (var plant in UnityEngine.Object.FindObjectsByType<Plant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (plant == null || !Live(plant.m_nview) || plant.m_status != Plant.Status.Healthy || !WillBePickable(plant))
            {
                continue;
            }

            var position = plant.transform.position;
            var offset = position - origin;
            if (offset.sqrMagnitude > rangeSqr)
            {
                continue;
            }

            found.Add(new HarvestTarget(position, offset.magnitude, false, null));
        }

        return found;
    }

    static bool Live(ZNetView? view)
    {
        return view != null && view.IsValid();
    }

    static bool WillBePickable(Plant plant)
    {
        var prefabs = plant.m_grownPrefabs;
        if (prefabs == null)
        {
            return false;
        }

        for (var i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            if (prefab == null)
            {
                continue;
            }

            if (!GrowsIntoPickable.TryGetValue(prefab, out var grows))
            {
                grows = prefab.GetComponentInChildren<Pickable>(true) != null;
                GrowsIntoPickable[prefab] = grows;
            }

            if (grows)
            {
                return true;
            }
        }

        return false;
    }

    static bool Ready(Pickable pickable)
    {
        return Live(pickable.m_nview) && pickable.CanBePicked();
    }

    static bool StuckInTar(Pickable pickable)
    {
        if (!pickable.m_tarPreventsPicking)
        {
            return false;
        }

        var floating = pickable.GetComponent<Floating>();
        return floating != null && floating.IsInTar();
    }

    /// <summary>
    /// Sends Valheim's own pick for a pickable that is ready. The owner spawns the drops and
    /// marks it picked, so success here means "the pick was issued", the same as a hand pick.
    /// </summary>
    static bool TryPick(Pickable pickable, Player player)
    {
        if (pickable == null || !Ready(pickable) || StuckInTar(pickable))
        {
            return false;
        }

        pickable.Interact(player, false, false);
        return true;
    }

    static bool TryPickItem(PickableItem item, Player player)
    {
        if (item == null || item.m_picked || !Live(item.m_nview))
        {
            return false;
        }

        return item.Interact(player, false, false);
    }
}
