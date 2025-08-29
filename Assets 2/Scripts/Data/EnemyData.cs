using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; } = 30;
    [field: SerializeField] public int AttackPower { get; private set; } = 6;

    [Header("animations")]
    [SerializeField] private RuntimeAnimatorController enemyAnimatorController;

    public RuntimeAnimatorController AnimatorController => enemyAnimatorController;

    [Header("AI / Moveset")]
    [SerializeField] private List<EnemyActionEntryData> moveset = new();
    [Tooltip("Pick next action at spawn and after each turn for intent UI.")]
    public bool PlanIntentOnSpawn = true;

    // Convert data entries → runtime entries the AI uses
    public List<EnemyActionEntry> BuildRuntimeMoveset()
    {
        var result = new List<EnemyActionEntry>(moveset.Count);
        foreach (var d in moveset)
        {
            if (d == null || d.action == null) continue;
            var e = new EnemyActionEntry
            {
                action         = d.action,
                weight         = Mathf.Max(1, d.weight),
                cooldownTurns  = Mathf.Max(0, d.cooldownTurns),
                maxUses        = d.maxUses,
                conditions     = d.conditions != null ? new List<EnemyCondition>(d.conditions) : new List<EnemyCondition>()
            };
            result.Add(e);
        }
        return result;
    }
}

[Serializable]
public class EnemyActionEntryData
{
    public EnemyAction action;
    [Min(1)] public int weight = 1;
    [Min(0)] public int cooldownTurns = 0;
    public int maxUses = -1;  // -1 = unlimited
    public List<EnemyCondition> conditions = new();
}