using UnityEngine;

public abstract class EnemyCondition : ScriptableObject
{
    public abstract bool Evaluate(EnemyView self);
}

[CreateAssetMenu(menuName = "EnemyAI/Conditions/HP Below %")]
public class HPBelowPercentCondition : EnemyCondition
{
    [Range(0f, 1f)] public float threshold = 0.5f;
    public override bool Evaluate(EnemyView self)
    {
        if (self == null || self.MaxHealth <= 0) return false;
        return self.CurrentHealth <= Mathf.CeilToInt(self.MaxHealth * threshold);
    }
}

[CreateAssetMenu(menuName = "EnemyAI/Conditions/Has Status (>= stacks)")]
public class HasStatusCondition : EnemyCondition
{
    public StatusEffectType type;
    public int minStacks = 1;
    public override bool Evaluate(EnemyView self)
    {
        return self != null && self.GetStatusEffectStacks(type) >= minStacks;
    }
}

[CreateAssetMenu(menuName = "EnemyAI/Conditions/Not Has Status")]
public class NotHasStatusCondition : EnemyCondition
{
    public StatusEffectType type;
    public override bool Evaluate(EnemyView self)
    {
        return self != null && self.GetStatusEffectStacks(type) == 0;
    }
}