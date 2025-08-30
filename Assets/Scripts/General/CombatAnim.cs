// CombatAnim.cs
using System.Collections;
using DG.Tweening;
using UnityEngine;

public static class CombatAnim
{
    public static IEnumerator StepForwardAndBackIfAlive(CombatantView actor, float offset = 1f, float fwd = 0.15f, float back = 0.25f)
    {
        if (SafeCombatant.AbortIfDead(actor, "step-forward(start)")) yield break;

        var t = actor.transform;
        var start = t.position;

        Tween forward = t.DOMoveX(start.x - offset, fwd);
        yield return forward.WaitForCompletion();

        if (SafeCombatant.AbortIfDead(actor, "step-forward(after)")) yield break;

        Tween backward = t.DOMoveX(start.x, back);
        yield return backward.WaitForCompletion();
    }
}