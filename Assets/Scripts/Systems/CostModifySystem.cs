using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ModifyCostSystem : MonoBehaviour
{
    [SerializeField] private bool tweenOnChange = true;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ModifyCostGA>(ModifyCostPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ModifyCostGA>();
    }

    private IEnumerator ModifyCostPerformer(ModifyCostGA ga)
    {
        // Resolve target: defaults to player (energy belongs to player)
        var player = PlayerSystem.Instance != null ? PlayerSystem.Instance.PlayerView : null;
        var target = ga.Target != null ? ga.Target : player;

        // If we somehow have no player/target, just bail safely
        if (target == null || target.Equals(null)) yield break;

        // Optional caster guards + animation
        var caster = ga.Caster;
        if (tweenOnChange && caster != null && !caster.Equals(null))
        {
            // Guard before anim
            if (SafeCombatant.AbortIfDead(caster, "ModifyCost(start)")) yield break;

            // Small hop
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);

            // Re-check after anim
            if (SafeCombatant.AbortIfDead(caster, "ModifyCost(after tween)")) yield break;
        }

        // Apply delta to current energy via CostSystem (clamped there)
        //CostSystem.Instance.ApplyDelta(ga.Delta);
        yield return null;
    }
}