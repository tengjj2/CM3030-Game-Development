using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyActions/Buff (Status Only)")]
public class BuffAction : EnemyAction
{

    [Header("Intent")]
    [SerializeField] private Sprite strengthIcon;
    [SerializeField] private Sprite weakenIcon;
    [SerializeField] private Sprite frailIcon;
    [SerializeField] private Sprite defenceIcon;
    [SerializeField] private Sprite burnIcon;
    [SerializeField] private Sprite poisonIcon;

    public override Sprite IntentSprite => BuffType switch
    {
        StatusEffectType.STRENGTH => strengthIcon,
        StatusEffectType.WEAKEN   => weakenIcon,
        StatusEffectType.FRAIL    => frailIcon,
        StatusEffectType.DEFENCE  => defenceIcon,
        StatusEffectType.BURN     => burnIcon,
        StatusEffectType.POISON   => poisonIcon,
        _                         => null
    };

    public override int? GetIntentValue(EnemyView self) => BuffStacks;

    [Header("Buff")]
    public StatusEffectType BuffType = StatusEffectType.STRENGTH;
    [Min(1)] public int BuffStacks = 1;

    [Header("Targeting")]
    public TargetingMode TargetMode = TargetingMode.Self;
    [Tooltip("Used only when TargetMode = SpecificEnemyIndex")]
    public int SpecificEnemyIndex = 0;

    public override void Enqueue(EnemyView self)
    {
        var targets = ResolveTargets(self, TargetMode, SpecificEnemyIndex);
        if (targets == null || targets.Count == 0 || BuffStacks <= 0) return;

        foreach (var t in targets)
            EnqueueApplyGA(self, BuffType, t, BuffStacks);
    }

    // ----------------- ROUTER -----------------
    private static void EnqueueApplyGA(EnemyView caster, StatusEffectType type, CombatantView target, int stacks)
    {
        if (target == null || stacks <= 0) return;

        switch (type)
        {
            // --- Stackable buffs/debuffs (pass stacks) ---
            case StatusEffectType.STRENGTH:
                // Variant A (target, stacks):
                ActionSystem.Instance.AddReaction(new ApplyStrengthGA(caster, caster, stacks));
                // Variant B (caster, target, stacks):
                // ActionSystem.Instance.AddReaction(new ApplyStrengthGA(caster, target, stacks));
                Debug.Log($"[BuffAction] STR +{stacks} → {caster.name}");
                break;

            case StatusEffectType.WEAKEN:
                ActionSystem.Instance.AddReaction(new ApplyWeakenGA(target, caster, stacks));
                // ActionSystem.Instance.AddReaction(new ApplyWeakenGA(caster, target, stacks));
                Debug.Log($"[BuffAction] WEAKEN +{stacks} → {target.name}");
                break;

            case StatusEffectType.DEFENCE:
                ActionSystem.Instance.AddReaction(new ApplyDefenceGA(caster, caster, stacks));
                // ActionSystem.Instance.AddReaction(new ApplyDefenceGA(caster, target, stacks));
                Debug.Log($"[BuffAction] DEFENCE +{stacks} → {caster.name}");
                break;

            case StatusEffectType.FRAIL:
                ActionSystem.Instance.AddReaction(new ApplyFrailGA(target, caster, stacks));
                // ActionSystem.Instance.AddReaction(new ApplyFrailGA(caster, target, stacks));
                Debug.Log($"[BuffAction] FRAIL +{stacks} → {target.name}");
                break;

            // --- DoT statuses (your project already uses (stacks, target)) ---
            case StatusEffectType.POISON:
                ActionSystem.Instance.AddReaction(new ApplyPoisonGA(target, caster, stacks));
                Debug.Log($"[BuffAction] POISON +{stacks} → {target.name}");
                break;

            case StatusEffectType.BURN:
                ActionSystem.Instance.AddReaction(new ApplyBurnGA(target, caster, stacks));
                Debug.Log($"[BuffAction] BURN +{stacks} → {target.name}");
                break;

            // Add cases here for any new statuses you introduce
            default:
                Debug.LogWarning($"[BuffAction] No Apply…GA mapped for {type}");
                break;
        }
    }

    // --------------- Target resolution ---------------
    private static List<CombatantView> ResolveTargets(EnemyView self, TargetingMode mode, int specificIndex)
    {
        var results = new List<CombatantView>();

        switch (mode)
        {
            case TargetingMode.Self:
                results.Add(self);
                break;

            case TargetingMode.Player:
                if (PlayerSystem.Instance?.PlayerView != null)
                    results.Add(PlayerSystem.Instance.PlayerView);
                break;

            case TargetingMode.RandomEnemy:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null && pool.Count > 0)
                {
                    var alive = new List<EnemyView>();
                    foreach (var e in pool) if (e != null && e.CurrentHealth > 0) alive.Add(e);
                    if (alive.Count > 0) results.Add(alive[Random.Range(0, alive.Count)]);
                }
                break;
            }

            case TargetingMode.AllEnemies:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null)
                    foreach (var e in pool)
                        if (e != null && e.CurrentHealth > 0) results.Add(e);
                break;
            }

            case TargetingMode.SpecificEnemyIndex:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null && pool.Count > 0)
                {
                    int idx = Mathf.Clamp(specificIndex, 0, pool.Count - 1);
                    var e = pool[idx];
                    if (e != null && e.CurrentHealth > 0) results.Add(e);
                }
                break;
            }
        }

        return results;
    }

    public enum TargetingMode { Self, Player, RandomEnemy, AllEnemies, SpecificEnemyIndex }
}