using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyActions/Heal")]
public class HealEnemyAction : EnemyAction
{
    [Header("Intent")]
    [SerializeField] private Sprite healIntentSprite;
    [Min(1)] public int Amount = 6;

    public override Sprite IntentSprite => healIntentSprite;
    public override int? GetIntentValue(EnemyView self) => Amount;

    [Header("Targeting")]
    public TargetingMode Mode = TargetingMode.Self;
    public int SpecificEnemyIndex = 0;

    public override void Enqueue(EnemyView self)
    {
        var targets = Resolve(self, Mode, SpecificEnemyIndex);
        if (targets == null || targets.Count == 0) return;

        foreach (var t in targets)
            ActionSystem.Instance.AddReaction(new ApplyHealGA(t, self, Amount));
    }

    // Reuse your existing resolver pattern
    private static List<CombatantView> Resolve(EnemyView self, TargetingMode mode, int idx)
    {
        var list = new List<CombatantView>();
        switch (mode)
        {
            case TargetingMode.Self:
                list.Add(self);
                break;

            case TargetingMode.Player:
                if (PlayerSystem.Instance?.PlayerView != null)
                    list.Add(PlayerSystem.Instance.PlayerView);
                break;

            case TargetingMode.RandomEnemy:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null)
                {
                    var alive = new List<EnemyView>();
                    foreach (var e in pool) if (e != null && e.CurrentHealth > 0) alive.Add(e);
                    if (alive.Count > 0) list.Add(alive[Random.Range(0, alive.Count)]);
                }
                break;
            }

            case TargetingMode.AllEnemies:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null)
                    foreach (var e in pool)
                        if (e != null && e.CurrentHealth > 0) list.Add(e);
                break;
            }

            case TargetingMode.SpecificEnemyIndex:
            {
                var pool = EnemySystem.Instance?.Enemies;
                if (pool != null && pool.Count > 0)
                {
                    int clamped = Mathf.Clamp(idx, 0, pool.Count - 1);
                    var e = pool[clamped];
                    if (e != null && e.CurrentHealth > 0) list.Add(e);
                }
                break;
            }
        }
        return list;
    }

    public enum TargetingMode { Self, Player, RandomEnemy, AllEnemies, SpecificEnemyIndex }
}