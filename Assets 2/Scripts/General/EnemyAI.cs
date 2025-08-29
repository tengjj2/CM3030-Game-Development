using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyActionEntry
{
    [Tooltip("The EnemyAction ScriptableObject to run (Attack/Block/Buff/etc.)")]
    public EnemyAction action;

    [Min(1)] public int weight = 1;        // Weighted random pick
    [Min(0)] public int cooldownTurns = 0; // Turns to wait after using this action
    public int maxUses = -1;               // -1 = unlimited uses

    [Tooltip("All conditions must pass for this action to be eligible.")]
    public List<EnemyCondition> conditions = new();

    // Runtime state
    [NonSerialized] public int cdRemaining;
    [NonSerialized] public int usesLeft;

    public void ResetRuntime()
    {
        cdRemaining = 0;
        usesLeft = (maxUses < 0) ? int.MaxValue : maxUses;
    }

    public bool IsEligible(EnemyView self)
    {
        if (action == null) return false;
        if (cdRemaining > 0) return false;
        if (usesLeft == 0) return false;

        if (conditions != null)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c != null && !c.Evaluate(self))
                    return false;
            }
        }
        return true;
    }
}

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private List<EnemyActionEntry> moves = new();  // keep serialized for debugging if you like
    public int MoveCount => moves?.Count ?? 0;
    public EnemyActionEntry NextPlanned { get; private set; }

    public void LoadFromData(List<EnemyActionEntry> entries)
    {
        moves = entries ?? new List<EnemyActionEntry>();
    }
    private bool _inited;

    public void Init()
    {
        if (_inited) { Debug.LogWarning($"[EnemyAI] Init twice on {name}"); return; }
        _inited = true;

        foreach (var e in moves) e.ResetRuntime();
        NextPlanned = ChooseNext(GetComponent<EnemyView>());
    }

    // --- Intent / choosing ---
    // Return the planned entry; DO NOT call back into EnemyView from here.
    public EnemyActionEntry PlanNextIntent(EnemyView self)
    {
        NextPlanned = ChooseNext(self);
        return NextPlanned;
    }

    public EnemyActionEntry ConsumePlanned(EnemyView self)
    {
        var chosen = NextPlanned ?? ChooseNext(self);
        if (chosen == null)
        {
            Debug.LogWarning($"[EnemyAI] {self.name} could not choose an action (no eligible moves).");
            return null;
        }

        Debug.Log($"[EnemyAI] {self.name} selected action: {(chosen.action != null ? chosen.action.name : "NULL")}");

        // Apply cooldown and use count now
        if (chosen.cooldownTurns > 0)
            chosen.cdRemaining = chosen.cooldownTurns + 1; // +1 so end-of-turn tick brings it to 'cooldownTurns'

        if (chosen.usesLeft > 0 && chosen.usesLeft < int.MaxValue)
            chosen.usesLeft--;

        NextPlanned = null; // force re-plan after the turn
        return chosen;
    }

    // --- End of turn cooldown tick ---
    public void TickEndOfTurn()
    {
        foreach (var e in moves)
            if (e.cdRemaining > 0) e.cdRemaining--;
    }

    // --- Weighted random, condition-aware chooser ---
    private EnemyActionEntry ChooseNext(EnemyView self)
    {
        var eligible = new List<EnemyActionEntry>();
        int totalWeight = 0;

        for (int i = 0; i < moves.Count; i++)
        {
            var e = moves[i];
            bool ok = e.IsEligible(self);
            Debug.Log($"[EnemyAI] {self.name} move[{i}] action={(e.action ? e.action.name : "NULL")} " +
                      $"eligible={ok} cd={e.cdRemaining} usesLeft={e.usesLeft} weight={e.weight} conds={e.conditions?.Count ?? 0}");
            if (ok) { eligible.Add(e); totalWeight += Mathf.Max(1, e.weight); }
        }

        if (eligible.Count == 0)
        {
            Debug.LogWarning($"[EnemyAI] {self.name} no ELIGIBLE moves — fix your cooldown/uses/conditions.");
            return null; // don’t silently pick the first non-null
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (var e in eligible)
        {
            roll -= Mathf.Max(1, e.weight);
            if (roll < 0) return e;
        }
        return eligible[eligible.Count - 1];
    }

    // --- Optional: get a UI string describing the next move ---
    public string GetIntentDescription()
    {
        return NextPlanned?.action != null ? NextPlanned.action.TelegraphText : string.Empty;
    }
}