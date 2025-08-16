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
            // POISON: apply at start (if this matches your design)
            int poisonStacks = self.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
            {
                ActionSystem.Instance.AddReaction(new DealDamageGA(poisonStacks, new() { self }, self));
                self.RemoveStatusEffect(StatusEffectType.POISON, poisonStacks);
                Debug.Log($"[Tick] POISON {tickStatusesGA.Target.name} takes {tickStatusesGA} at START");
            }
            /*
            else if (poisonStacks == 1)
            {
                ActionSystem.Instance.AddReaction(new DealDamageGA(poisonStacks, new() { self }, self));
                self.RemoveStatusEffect(StatusEffectType.POISON, poisonStacks);
            }*/

            int blockStacks = self.GetStatusEffectStacks(StatusEffectType.BLOCK);
            if (blockStacks > 0)
            {
                ActionSystem.Instance.AddReaction(new ApplyBlockGA(self, self, blockStacks));
                self.RemoveStatusEffect(StatusEffectType.BLOCK, blockStacks);
            }
            
            Decay(self, StatusEffectType.DEFENCE, 1);
            // if (block > 0) unit.RemoveStatusEffect(StatusEffectType.BLOCK, block);

            // If you have other start-of-turn statuses, enqueue them here...
        }

        // ----- END-OF-TURN effects -----
        if (tickStatusesGA.Phase == TickPhase.EndOfTurn)
        {
            // BURN: apply AFTER end turn click (end of owner's turn)
        int burnStacks = self.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ActionSystem.Instance.AddReaction(new DealDamageGA(burnStacks, new() { self }, self));
            // optional: decay burn per proc
            self.RemoveStatusEffect(StatusEffectType.BURN, burnStacks);
        }

            // Decays at the END of the owner’s turn (common pattern)
            if (tickStatusesGA.IsOwnersTurn)
            {
                Decay(self, StatusEffectType.WEAKEN, 1);
                Decay(self, StatusEffectType.FRAIL, 1);
                //Decay(self, StatusEffectType.DEFENCE, 1);
                Decay(self, StatusEffectType.STRENGTH, 1);
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