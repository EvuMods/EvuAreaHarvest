using UnityEngine;

namespace EvuAreaHarvest;

internal sealed class RangeRing
{
    const int Segments = 64;
    const float MoveTolerance = 0.05f;

    readonly LineRenderer _edge;
    readonly LineRenderer _ripple;
    readonly float[] _edgeY = new float[Segments];
    readonly Vector3[] _points = new Vector3[Segments];
    Vector3 _cachedCenter;
    float _cachedRange = -1f;
    float _centerY;

    public RangeRing(Transform parent)
    {
        _edge = GroundLine.Create("EvuAreaHarvest Range", parent, 0.12f);
        _ripple = GroundLine.Create("EvuAreaHarvest Ripple", parent, 0.07f);
    }

    public void Hide()
    {
        _edge.positionCount = 0;
        _ripple.positionCount = 0;
        _cachedRange = -1f;
    }

    /// <summary>
    /// Draws the range edge and the ripple. Ground heights are sampled only when <paramref name="refresh"/> is set,
    /// the player moved, or the range changed, so an idle frame costs no raycasts.
    /// </summary>
    public void Show(Vector3 center, float range, bool refresh)
    {
        if (refresh || !Mathf.Approximately(range, _cachedRange) || (center - _cachedCenter).sqrMagnitude > MoveTolerance * MoveTolerance)
        {
            Sample(center, range);
        }

        var pulse = 1f + (0.02f * Mathf.Sin(Time.time * 2.6f));
        Fill(center, range * pulse, 1f);
        GroundLine.Circle(_edge, _points, new Color(0.95f, 0.78f, 0.15f, 0.9f));

        var repeat = Mathf.Repeat(Time.time / 1.5f, 1f);
        Fill(center, range * repeat, repeat);
        GroundLine.Circle(_ripple, _points, new Color(0.95f, 0.82f, 0.25f, (1f - repeat) * 0.75f));
    }

    void Sample(Vector3 center, float range)
    {
        _cachedCenter = center;
        _cachedRange = range;
        _centerY = GroundLine.SurfaceY(center.x, center.z, center.y);
        for (var i = 0; i < Segments; i++)
        {
            var angle = i / (float)Segments * Mathf.PI * 2f;
            var x = center.x + (Mathf.Cos(angle) * range);
            var z = center.z + (Mathf.Sin(angle) * range);
            _edgeY[i] = GroundLine.SurfaceY(x, z, center.y);
        }
    }

    /// <summary>
    /// Fills the point buffer for a ring of <paramref name="radius"/>. The height is blended from the ground under the
    /// player to the sampled edge height by <paramref name="t"/>, which is the ring's fraction of the full range.
    /// </summary>
    void Fill(Vector3 center, float radius, float t)
    {
        for (var i = 0; i < Segments; i++)
        {
            var angle = i / (float)Segments * Mathf.PI * 2f;
            var x = center.x + (Mathf.Cos(angle) * radius);
            var z = center.z + (Mathf.Sin(angle) * radius);
            var y = Mathf.Lerp(_centerY, _edgeY[i], t) + GroundLine.Lift;
            _points[i] = new Vector3(x, y, z);
        }
    }
}
