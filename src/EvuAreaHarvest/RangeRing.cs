using UnityEngine;

namespace EvuAreaHarvest;

internal sealed class RangeRing
{
    const int Segments = 64;

    readonly LineRenderer _edge;
    readonly LineRenderer _ripple;

    public RangeRing(Transform parent)
    {
        _edge = GroundLine.Create("EvuAreaHarvest Range", parent, 0.12f);
        _ripple = GroundLine.Create("EvuAreaHarvest Ripple", parent, 0.07f);
    }

    public void Hide()
    {
        _edge.positionCount = 0;
        _ripple.positionCount = 0;
    }

    public void Show(Vector3 center, float range)
    {
        var pulse = 1f + (0.02f * Mathf.Sin(Time.time * 2.6f));
        GroundLine.Circle(_edge, center, range * pulse, new Color(0.95f, 0.78f, 0.15f, 0.9f), Segments, center.y);
        var repeat = Mathf.Repeat(Time.time / 1.5f, 1f);
        var ripple = new Color(0.95f, 0.82f, 0.25f, (1f - repeat) * 0.75f);
        GroundLine.Circle(_ripple, center, range * repeat, ripple, Segments, center.y);
    }
}
