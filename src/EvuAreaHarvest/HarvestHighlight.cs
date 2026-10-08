using UnityEngine;

namespace EvuAreaHarvest;

internal sealed class HarvestHighlight
{
    const float ScanInterval = 0.25f;

    readonly RangeRing _range;
    readonly HarvestMarkers _markers;
    float _nextScan = -1f;

    public HarvestHighlight(Transform parent)
    {
        _range = new RangeRing(parent);
        _markers = new HarvestMarkers(parent);
    }

    public void Tick(Player player, bool show, float range, bool pickGuarded)
    {
        if (!show)
        {
            Hide();
            _nextScan = -1f;
            return;
        }

        var origin = player.transform.position;
        var scan = Time.time >= _nextScan;
        if (scan)
        {
            _markers.Show(HarvestScan.Collect(origin, range, pickGuarded));
            _nextScan = Time.time + ScanInterval;
        }

        _range.Show(origin, range, scan);
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
