// BlockAction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyActions/Block")]
public class BlockAction : EnemyAction
{
    [Header("Intent")]
    [SerializeField] private Sprite blockIcon;
    public override Sprite IntentSprite => blockIcon;
    public override int? GetIntentValue(EnemyView self) => amount;

    [Header("Block")]
    [Min(0)] public int amount = 8;

    [Header("Targeting")]
    public BuffAction.TargetingMode targetMode = BuffAction.TargetingMode.Self;
    public int specificEnemyIndex = 0;

    public override void Enqueue(EnemyView self)
    {
        if (!SafeCombatant.IsAlive(self) || amount <= 0) return;

        var targets = ResolveTargets(self, targetMode, specificEnemyIndex);
        if (targets == null || targets.Count == 0) return;

        foreach (var t in targets)
        {
            ActionSystem.Instance.AddReaction(new ApplyBlockGA(
                target: t,
                caster: self,
                baseAmount: amount
            ));
            Debug.Log($"[BlockAction] BLOCK +{amount} → {t.name}");
        }
    }

    // Simple reuse of your targeting helper
    private static System.Collections.Generic.List<CombatantView> ResolveTargets(EnemyView self, BuffAction.TargetingMode mode, int specificIndex)
        => (System.Collections.Generic.List<CombatantView>)typeof(BuffAction)
           .GetMethod("ResolveTargets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
           .Invoke(null, new object[] { self, mode, specificIndex });
}
