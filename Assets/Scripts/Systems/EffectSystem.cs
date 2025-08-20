// EffectSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }

    private IEnumerator PerformEffectPerformer(PerformEffectGA ga)
    {
        var caster = ga.Caster != null ? ga.Caster : PlayerSystem.Instance.PlayerView; // fallback
        var targets = ga.Targets;

        // If the PLAYER is confused, randomize the targets list (incl. self)
        if (caster == PlayerSystem.Instance.PlayerView)
        {
            int confuseStacks = caster.GetStatusEffectStacks(StatusEffectType.CONFUSE);
            if (confuseStacks > 0)
            {
                targets = RandomizeTargetsKeepingCount(targets);
                Debug.Log($"[Confuse] Player confused → randomized targets to count={targets?.Count ?? 0}");
            }
        }

        // Now ask the Effect to produce its GA with (targets, caster)
        GameAction effectAction = ga.Effect.GetGameAction(targets, caster);
        if (effectAction != null)
            ActionSystem.Instance.AddReaction(effectAction);

        yield return null;
    }

    // Helpers
    private static List<CombatantView> RandomizeTargetsKeepingCount(List<CombatantView> original)
    {
        int want = Mathf.Max(1, original?.Count ?? 1); // at least 1
        var pool = AllLivingCombatants();
        if (pool.Count == 0) return original;

        // For single target, pick one random; for N, pick N random (allow duplicates or not?)
        // Here: no duplicates unless pool smaller than want.
        var result = new List<CombatantView>(want);
        var bag = new List<CombatantView>(pool);
        for (int i = 0; i < want; i++)
        {
            if (bag.Count == 0) bag = new List<CombatantView>(pool); // fallback if want > pool
            int idx = Random.Range(0, bag.Count);
            result.Add(bag[idx]);
            bag.RemoveAt(idx);
        }
        return result;
    }

    private static List<CombatantView> AllLivingCombatants()
    {
        var list = new List<CombatantView>();
        var player = PlayerSystem.Instance?.PlayerView;
        if (player != null && player.CurrentHealth > 0) list.Add(player);

        var enemies = EnemySystem.Instance?.Enemies;
        if (enemies != null)
        {
            foreach (var e in enemies)
                if (e != null && e.CurrentHealth > 0) list.Add(e);
        }
        return list;
    }
}