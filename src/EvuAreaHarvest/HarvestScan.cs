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
    enum PrefabKind
    {
        None,
        Pickable,
        PickableItem,
        Plant,
    }

    static readonly Dictionary<int, bool> GrowsIntoPickable = new Dictionary<int, bool>();
    static readonly Dictionary<int, PrefabKind> PrefabKinds = new Dictionary<int, PrefabKind>();

    /// <summary>
    /// Everything inside <paramref name="range"/> of <paramref name="origin"/> that is, or will become, a pickable.
    /// Walks Valheim's live instance registry. <paramref name="visited"/> is how many instances were considered.
    /// </summary>
    public static List<HarvestTarget> Collect(Vector3 origin, float range, bool pickGuarded, out int visited)
    {
        var found = new List<HarvestTarget>();
        visited = 0;
        var scene = ZNetScene.instance;
        if (scene == null)
        {
            return found;
        }

        var rangeSqr = range * range;
        var coarse = range + 2f;
        var coarseSqr = coarse * coarse;
        foreach (var pair in scene.m_instances)
        {
            visited++;
            var view = pair.Value;
            var zdo = pair.Key;
            if (view == null || zdo == null || !view.IsValid())
            {
                continue;
            }

            var rough = zdo.GetPosition() - origin;
            if (rough.sqrMagnitude > coarseSqr)
            {
                continue;
            }

            switch (Kind(scene, zdo.GetPrefab()))
            {
                case PrefabKind.Pickable:
                    ConsiderPickable(view, origin, rangeSqr, pickGuarded, found);
                    break;
                case PrefabKind.PickableItem:
                    ConsiderItem(view, origin, rangeSqr, found);
                    break;
                case PrefabKind.Plant:
                    ConsiderPlant(view, origin, rangeSqr, found);
                    break;
            }
        }

        return found;
    }

    static PrefabKind Kind(ZNetScene scene, int hash)
    {
        if (PrefabKinds.TryGetValue(hash, out var kind))
        {
            return kind;
        }

        var prefab = scene.GetPrefab(hash);
        if (prefab == null)
        {
            kind = PrefabKind.None;
        }
        else if (prefab.GetComponentInChildren<Pickable>(true) != null)
        {
            kind = PrefabKind.Pickable;
        }
        else if (prefab.GetComponentInChildren<PickableItem>(true) != null)
        {
            kind = PrefabKind.PickableItem;
        }
        else if (prefab.GetComponentInChildren<Plant>(true) != null)
        {
            kind = PrefabKind.Plant;
        }
        else
        {
            kind = PrefabKind.None;
        }

        PrefabKinds[hash] = kind;
        return kind;
    }

    static void ConsiderPickable(ZNetView view, Vector3 origin, float rangeSqr, bool pickGuarded, List<HarvestTarget> found)
    {
        var pickable = view.GetComponent<Pickable>();
        if (pickable == null || pickable.m_itemPrefab == null || pickable.m_enabled == 0)
        {
            return;
        }

        if (!pickGuarded && pickable.m_aggravateRange > 0f)
        {
            return;
        }

        var position = pickable.transform.position;
        var offset = position - origin;
        if (offset.sqrMagnitude > rangeSqr)
        {
            return;
        }

        var captured = pickable;
        found.Add(new HarvestTarget(
            position,
            offset.magnitude,
            Ready(captured),
            player => TryPick(captured, player)));
    }

    static void ConsiderItem(ZNetView view, Vector3 origin, float rangeSqr, List<HarvestTarget> found)
    {
        var item = view.GetComponent<PickableItem>();
        if (item == null || item.m_itemPrefab == null)
        {
            return;
        }

        var position = item.transform.position;
        var offset = position - origin;
        if (offset.sqrMagnitude > rangeSqr)
        {
            return;
        }

        var captured = item;
        found.Add(new HarvestTarget(
            position,
            offset.magnitude,
            !item.m_picked,
            player => TryPickItem(captured, player)));
    }

    static void ConsiderPlant(ZNetView view, Vector3 origin, float rangeSqr, List<HarvestTarget> found)
    {
        var plant = view.GetComponent<Plant>();
        if (plant == null || plant.m_status != Plant.Status.Healthy || !WillBePickable(plant))
        {
            return;
        }

        var position = plant.transform.position;
        var offset = position - origin;
        if (offset.sqrMagnitude > rangeSqr)
        {
            return;
        }

        found.Add(new HarvestTarget(position, offset.magnitude, false, null));
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

            var id = prefab.GetInstanceID();
            if (!GrowsIntoPickable.TryGetValue(id, out var grows))
            {
                grows = prefab.GetComponentInChildren<Pickable>(true) != null;
                GrowsIntoPickable[id] = grows;
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
