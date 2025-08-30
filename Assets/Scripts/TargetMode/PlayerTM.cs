using UnityEngine;
using System.Collections.Generic;

public class PlayerTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        List<CombatantView> targets = new()
        {
            PlayerSystem.Instance.PlayerView
        };
        return targets;
    }
}
