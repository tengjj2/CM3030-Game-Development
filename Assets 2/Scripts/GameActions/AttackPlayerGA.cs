using UnityEngine;

public class AttackPlayerGA : GameAction, IHaveCaster
{
    public EnemyView Attacker { get; private set; }
    public string DebugSource { get; }  // <-- add this
    public CombatantView Caster { get; private set; }
    public AttackPlayerGA(EnemyView attacker, string debugSource = null)
    {
        Attacker = attacker;
        Caster = Attacker;
        DebugSource = debugSource;
    }
}
