// SceneDimmer.cs
using UnityEngine;
using System.Collections.Generic;

public class SceneDimmer : Singleton<SceneDimmer>
{
    private readonly List<(SpriteRenderer sr, Color original)> cached = new();
    private bool dimmed = false;

    public void DimExcept(Transform keepRoot, float factor = 0.4f)
    {
        if (dimmed) return;
        cached.Clear();
        var all = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in all)
        {
            if (sr == null) continue;
            if (keepRoot != null && sr.transform.IsChildOf(keepRoot)) continue;
            cached.Add((sr, sr.color));
            Color c = sr.color;
            c.r *= factor; c.g *= factor; c.b *= factor;
            sr.color = c;
        }
        dimmed = true;
    }

    public void Undim()
    {
        if (!dimmed) return;
        foreach (var (sr, original) in cached)
            if (sr != null) sr.color = original;
        cached.Clear();
        dimmed = false;
    }
}