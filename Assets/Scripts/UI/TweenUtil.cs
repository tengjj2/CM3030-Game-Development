using DG.Tweening;
using UnityEngine;

public static class TweenUtil
{
    /// Kill all tweens on this transform AND all children (safe even if null).
    public static void KillTweensRecursive(this Transform t)
    {
        if (t == null) return;
        // Kill tweens targeting this transform
        t.DOKill(false);

        // Kill tweens targeting common components that may be animated
        foreach (var tr in t.GetComponentsInChildren<Transform>(true))
            tr.DOKill(false);

        // Also kill by target GameObject in case any code used GO as tween target
        DOTween.Kill(t, false);
        DOTween.Kill(t.gameObject, false);
    }
}