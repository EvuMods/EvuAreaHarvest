using UnityEngine;

namespace EvuAreaHarvest;

internal static class GroundLine
{
    /// <summary>How far above the sampled surface a line is drawn, to avoid z-fighting with the ground.</summary>
    public const float Lift = 0.08f;

    const float MinRadius = 0.05f;

    static Material? _material;
    static int _mask = int.MinValue;

    public static Material Material
    {
        get { return _material ?? (_material = CreateMaterial()); }
    }

    public static LineRenderer Create(string name, Transform parent, float width)
    {
        var marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        var line = marker.AddComponent<LineRenderer>();
        line.material = Material;
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = width;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.positionCount = 0;
        return line;
    }

    /// <summary>Draws a ring on the ground around <paramref name="center"/>, sampling the surface at every segment.</summary>
    public static void Circle(LineRenderer line, Vector3 center, float radius, Color color, int segments, float hintY)
    {
        if (radius <= MinRadius)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = segments;
        line.loop = true;
        line.startColor = color;
        line.endColor = color;
        for (var i = 0; i < segments; i++)
        {
            var angle = i / (float)segments * Mathf.PI * 2f;
            var x = center.x + (Mathf.Cos(angle) * radius);
            var z = center.z + (Mathf.Sin(angle) * radius);
            var y = SurfaceY(x, z, hintY) + Lift;
            line.SetPosition(i, new Vector3(x, y, z));
        }
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
        if (_material == null)
        {
            return;
        }

        UnityEngine.Object.Destroy(_material);
        _material = null;
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
    /// A vertex-colored, unlit material for the rings. Built-in shaders come first so the look does not depend on
    /// whichever LineRenderer the game happened to load. Borrowing one is the last resort.
    /// </summary>
    static Material CreateMaterial()
    {
        var shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Hidden/Internal-Colored")
            ?? Shader.Find("GUI/Text Shader");
        if (shader != null)
        {
            return new Material(shader);
        }

        var existing = UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < existing.Length; i++)
        {
            var shared = existing[i].sharedMaterial;
            if (shared != null && shared.shader != null)
            {
                Plugin.Log.LogWarning("No built-in line shader found. Borrowing the material of " + existing[i].name + " for the highlight.");
                return new Material(shared);
            }
        }

        Plugin.Log.LogWarning("No line shader found. The highlight may render without color.");
        return new Material(Shader.Find("Hidden/InternalErrorShader"));
    }
}
