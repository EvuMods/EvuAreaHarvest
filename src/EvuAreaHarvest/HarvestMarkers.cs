using System.Collections.Generic;
using UnityEngine;

namespace EvuAreaHarvest;

/// <summary>
/// v0 markers. Replace this type when the per-harvestable highlight changes.
/// </summary>
internal sealed class HarvestMarkers
{
    const int Segments = 16;
    const float Radius = 0.4f;

    readonly Transform _parent;
    readonly List<LineRenderer> _pool = new List<LineRenderer>();

    public HarvestMarkers(Transform parent)
    {
        _parent = parent;
    }

    public void Hide()
    {
        for (var i = 0; i < _pool.Count; i++)
        {
            _pool[i].positionCount = 0;
        }
    }

    public void Show(IReadOnlyList<HarvestTarget> spots)
    {
        for (var i = 0; i < spots.Count; i++)
        {
            var spot = spots[i];
            var color = spot.Ready
                ? new Color(0.95f, 0.78f, 0.15f, 0.95f)
                : new Color(0.55f, 0.42f, 0.12f, 0.4f);
            // The spot's own height is the hint, so a marker uphill or downhill from the player still finds its ground.
            GroundLine.Circle(Rent(i), spot.Position, Radius, color, Segments, spot.Position.y);
        }

        for (var i = spots.Count; i < _pool.Count; i++)
        {
            _pool[i].positionCount = 0;
        }
    }

    LineRenderer Rent(int index)
    {
        while (_pool.Count <= index)
        {
            _pool.Add(GroundLine.Create("EvuAreaHarvest Marker", _parent, 0.05f));
        }

        return _pool[index];
    }
}
