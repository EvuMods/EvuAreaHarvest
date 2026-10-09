using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EvuAreaHarvest;

/// <summary>
/// Flat sky-facing rings, one mesh for every harvestable in range. Rebuilt when the scan runs.
/// </summary>
internal sealed class HarvestMarkers
{
    const int Segments = 16;
    const float Outer = 0.4f;
    const float Width = 0.1f;

    static readonly Color ReadyColor = new Color(0.95f, 0.78f, 0.15f, 0.95f);
    static readonly Color WaitingColor = new Color(0.55f, 0.42f, 0.12f, 0.4f);

    readonly Transform _root;
    readonly Mesh _mesh;
    readonly MeshRenderer _renderer;
    readonly List<Vector3> _vertices = new List<Vector3>();
    readonly List<Color> _colors = new List<Color>();
    readonly List<int> _triangles = new List<int>();

    public HarvestMarkers(Transform parent)
    {
        var marker = new GameObject("EvuAreaHarvest Markers");
        _root = marker.transform;
        _root.SetParent(parent, false);
        // Vertices are written in world space, so the mesh object stays at the origin.
        _root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _root.localScale = Vector3.one;
        var filter = marker.AddComponent<MeshFilter>();
        _renderer = marker.AddComponent<MeshRenderer>();
        _mesh = new Mesh { name = "EvuAreaHarvest Markers" };
        _mesh.MarkDynamic();
        filter.sharedMesh = _mesh;
        _renderer.sharedMaterial = GroundLine.Material;
        _renderer.shadowCastingMode = ShadowCastingMode.Off;
        _renderer.receiveShadows = false;
        _renderer.lightProbeUsage = LightProbeUsage.Off;
        _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        _renderer.enabled = false;
    }

    public void Hide()
    {
        _mesh.Clear();
        _renderer.enabled = false;
    }

    public void Show(IReadOnlyList<HarvestTarget> spots)
    {
        _root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _root.localScale = Vector3.one;
        _vertices.Clear();
        _colors.Clear();
        _triangles.Clear();
        var inner = Outer - Width;
        for (var s = 0; s < spots.Count; s++)
        {
            var spot = spots[s];
            var color = spot.Ready ? ReadyColor : WaitingColor;
            var y = GroundLine.SurfaceY(spot.Position.x, spot.Position.z, spot.Position.y) + GroundLine.Lift;
            var start = _vertices.Count;
            for (var i = 0; i < Segments; i++)
            {
                var angle = i / (float)Segments * Mathf.PI * 2f;
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);
                _vertices.Add(new Vector3(spot.Position.x + (cos * inner), y, spot.Position.z + (sin * inner)));
                _vertices.Add(new Vector3(spot.Position.x + (cos * Outer), y, spot.Position.z + (sin * Outer)));
                _colors.Add(color);
                _colors.Add(color);
                var innerHere = start + (i * 2);
                var outerHere = innerHere + 1;
                var innerNext = start + (((i + 1) % Segments) * 2);
                var outerNext = innerNext + 1;
                // Clockwise when seen from above. Unity treats that winding as the front face.
                _triangles.Add(innerHere);
                _triangles.Add(outerNext);
                _triangles.Add(outerHere);
                _triangles.Add(innerHere);
                _triangles.Add(innerNext);
                _triangles.Add(outerNext);
            }
        }

        _mesh.Clear();
        if (_vertices.Count == 0)
        {
            _renderer.enabled = false;
            return;
        }

        _mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        _mesh.SetVertices(_vertices);
        _mesh.SetColors(_colors);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.RecalculateBounds();
        _renderer.enabled = true;
    }
}
