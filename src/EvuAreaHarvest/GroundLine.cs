using UnityEngine;

namespace EvuAreaHarvest;

internal static class GroundLine
{
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

    public static void Circle(LineRenderer line, Vector3 center, float radius, Color color, int segments, float hintY)
    {
        if (radius <= 0.05f)
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
            var x = center.x + Mathf.Cos(angle) * radius;
            var z = center.z + Mathf.Sin(angle) * radius;
            var y = SurfaceY(x, z, hintY) + 0.08f;
            line.SetPosition(i, new Vector3(x, y, z));
        }
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

    static float SurfaceY(float x, float z, float hintY)
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

    static Material CreateMaterial()
    {
        var existing = UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < existing.Length; i++)
        {
            var shared = existing[i].sharedMaterial;
            if (shared != null && shared.shader != null)
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
