using UnityEngine;

namespace EvuAreaHarvest;

internal static class GroundLine
{
    /// <summary>How far above the sampled surface a line is drawn, to avoid z-fighting with the ground.</summary>
    public const float Lift = 0.08f;

    static Material? _material;
    static int _mask = int.MinValue;

    internal static Material Material
    {
        get { return _material ?? (_material = CreateMaterial()); }
    }

    public static LineRenderer Create(string name, Transform parent, float width)
    {
        // Resolve before AddComponent, so the search does not clone the default material on the line being created.
        var material = Material;
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        var line = marker.AddComponent<LineRenderer>();
        line.material = material;
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = width;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.positionCount = 0;
        return line;
    }

    /// <summary>Draws a ring from points the caller already placed on the ground.</summary>
    public static void Circle(LineRenderer line, Vector3[] points, Color color)
    {
        line.positionCount = points.Length;
        line.loop = true;
        line.startColor = color;
        line.endColor = color;
        line.SetPositions(points);
    }

    public static void DestroyMaterial()
    {
        if (_material != null)
        {
            UnityEngine.Object.Destroy(_material);
            _material = null;
        }
    }

    public static float SurfaceY(float x, float z, float hintY)
    {
        var origin = new Vector3(x, hintY + 12f, z);
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 48f, Mask(), QueryTriggerInteraction.Ignore))
        {
            return hit.point.y;
        }

        if (ZoneSystem.instance != null)
        {
            return ZoneSystem.instance.GetGroundHeight(new Vector3(x, hintY, z));
        }

        return hintY;
    }

    static int Mask()
    {
        if (_mask != int.MinValue)
        {
            return _mask;
        }

        var named = LayerMask.GetMask("terrain", "Default", "static_solid", "piece", "piece_nonsolid");
        _mask = named == 0 ? Physics.DefaultRaycastLayers : named;
        return _mask;
    }

    /// <summary>
    /// A material Valheim already draws lines with. Built-in shaders are the fallback when the scene has none yet.
    /// </summary>
    static Material CreateMaterial()
    {
        var existing = UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < existing.Length; i++)
        {
            var shared = existing[i].sharedMaterial;
            if (shared != null && shared.shader != null && shared.shader.name != "Hidden/InternalErrorShader")
            {
                return new Material(shared);
            }
        }

        var shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("GUI/Text Shader")
            ?? Shader.Find("Hidden/Internal-Colored")
            ?? Shader.Find("Standard");
        return new Material(shader);
    }
}
