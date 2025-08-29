using System.Collections;
using UnityEngine;

public class StatusTickSystem : Singleton<StatusTickSystem>
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<TickStatusesGA>(TickPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<TickStatusesGA>();
    }

    private IEnumerator TickPerformer(TickStatusesGA tickStatusesGA)
    {
        var self = tickStatusesGA.Target;
        var player = PlayerSystem.Instance.PlayerView;

        // ----- START-OF-TURN effects -----
        if (tickStatusesGA.Phase == TickPhase.StartOfTurn)
        {
            var unit = tickStatusesGA.Target;
            if (unit == PlayerSystem.Instance.PlayerView)
            {
                int loss = unit.GetStatusEffectStacks(StatusEffectType.ENERGYLOSS);
                if (loss > 0)
                {
                    // reduce AFTER refill (TurnSystem queues Refill before this Tick)
                    ActionSystem.Instance.AddReaction(new ModifyCostGA(-loss));
                    unit.RemoveStatusEffect(StatusEffectType.ENERGYLOSS, loss);
                    Debug.Log($"[Tick] ENERGYLOSS applied: -{loss}");
                }
            }

            // POISON: apply at start (if this matches your design)
            int poisonStacks = self.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
            {
                ActionSystem.Instance.AddReaction(new DealDamageGA(poisonStacks, new() { self }, self));
                self.RemoveStatusEffect(StatusEffectType.POISON, 1);
                Debug.Log($"[Tick] POISON {tickStatusesGA.Target.name} takes {tickStatusesGA} at START");
            }

            int blockStacks = self.GetStatusEffectStacks(StatusEffectType.BLOCK);
            if (blockStacks > 0)
            {
                self.RemoveStatusEffect(StatusEffectType.BLOCK, blockStacks);
                Debug.Log($"[Tick] {self.name} BLOCK cleared ({blockStacks} → 0)");
            }


            int thornStacks = self.GetStatusEffectStacks(StatusEffectType.THORNS);
            if (thornStacks > 0)
            {
                self.RemoveStatusEffect(StatusEffectType.THORNS, thornStacks);
            }
            
            Decay(self, StatusEffectType.DEFENCE, 1);
            // if (block > 0) unit.RemoveStatusEffect(StatusEffectType.BLOCK, block);

            // If you have other start-of-turn statuses, enqueue them here...
        }

        // ----- END-OF-TURN effects -----
        if (tickStatusesGA.Phase == TickPhase.EndOfTurn)
        {  
            CostSystem.Instance.ClearOverflow();
            var unit = tickStatusesGA.Target;
            int gainVis = unit.GetStatusEffectStacks(StatusEffectType.ENERGYGAIN);
            if (gainVis > 0)
            {
                unit.RemoveStatusEffect(StatusEffectType.ENERGYGAIN, gainVis);
                Debug.Log($"[Tick] ENERGYGAIN cleared (visual): -{gainVis}");
            }
                // BURN: apply AFTER end turn click (end of owner's turn)
            int burnStacks = self.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ActionSystem.Instance.AddReaction(new DealDamageGA(burnStacks, new() { self }, self));
                // optional: decay burn per proc
                self.RemoveStatusEffect(StatusEffectType.BURN, burnStacks);
            }

            if (tickStatusesGA.IsOwnersTurn)
            {
                // CONFUSE: remove ALL stacks at end of the owner's turn
                int confuse = unit.GetStatusEffectStacks(StatusEffectType.CONFUSE);
                if (confuse > 0)
                {
                    unit.RemoveStatusEffect(StatusEffectType.CONFUSE, confuse);

                    // If it's an enemy, refresh intent UI (back to real intent)
                    if (unit is EnemyView ev) ev.RefreshIntentUI();
                }

                // Keep your other end-of-turn decays as-is
                Decay(unit, StatusEffectType.WEAKEN, 1);
                Decay(unit, StatusEffectType.FRAIL, 1);
                Decay(unit, StatusEffectType.STRENGTH, 1);
                // Decay(unit, StatusEffectType.DEFENCE, 1);
            }
        }

        yield return null;
    }
    
    private static void Decay(CombatantView v, StatusEffectType t, int amt)
    {
        int s = v.GetStatusEffectStacks(t);
        if (s > 0) v.RemoveStatusEffect(t, amt);
    }
}