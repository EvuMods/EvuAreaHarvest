using System.Collections.Generic;
using EvuAreaHarvest.Core;
using UnityEngine;

namespace EvuAreaHarvest;

internal sealed class HarvestHighlight
{
    readonly RangeRing _range;
    readonly HarvestMarkers _markers;
    readonly List<HarvestTarget> _spots = new List<HarvestTarget>();
    float _nextScan = -1f;

    public HarvestHighlight(Transform parent)
    {
        _range = new RangeRing(parent);
        _markers = new HarvestMarkers(parent);
    }

    public void Tick(Player player, bool show, float range)
    {
        if (!show)
        {
            Hide();
            _nextScan = -1f;
            return;
        }

        var origin = player.transform.position;
        if (Time.time >= _nextScan)
        {
            _spots.Clear();
            var found = HarvestScan.Collect(origin);
            for (var i = 0; i < found.Count; i++)
            {
                if (HarvestMath.InRange(found[i].Distance, range))
                {
                    _spots.Add(found[i]);
                }
            }

            _markers.Show(_spots, origin.y);
            _nextScan = Time.time + 0.25f;
        }

        _range.Show(origin, range);
    }

    public void Hide()
    {
        _range.Hide();
        _markers.Hide();
    }

    public void Destroy()
    {
        Hide();
        GroundLine.DestroyMaterial();
    }
}
