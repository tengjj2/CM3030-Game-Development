// CombatEndSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEndSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<CombatVictoryGA>(VictoryPerformer);
        ActionSystem.AttachPerformer<CombatDefeatGA>(DefeatPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<CombatVictoryGA>();
        ActionSystem.DetachPerformer<CombatDefeatGA>();
    }

    private IEnumerator VictoryPerformer(CombatVictoryGA ga)
    {
        // Pause turns (no more Enemy/Player actions)
        TurnSystem.Instance?.SuspendCombat();

        // Optional: small pause for drama
        yield return new WaitForSeconds(0.25f);

        // MONEY
        if (ga.Gold > 0)
            PlayerSystem.Instance?.AddGold(ga.Gold);

        // HEAL (use your HealSystem/GA if you prefer)
        var pv = PlayerSystem.Instance?.PlayerView;
        if (pv != null && ga.HealAmount > 0)
        {
            pv.CurrentHealth = Mathf.Min(pv.CurrentHealth + ga.HealAmount, pv.MaxHealth);
            pv.RefreshHealthUI(); // or pv.UpdateHealthUI() — call your existing refresh
        }

        // SHOW UI (cards, gold, heal summary)
        var ui = CombatEndUI.Instance;
        if (ui == null)
        {
            Debug.LogWarning("[CombatEndSystem] No CombatEndUI in scene — advancing immediately.");
            RunManager.Instance?.NextFloor();
            yield break;
        }

        // If there are card picks, run a choose flow then enable the Next button.
        if (ga.PickCardCount > 0 && ga.CardRewardPool != null && ga.CardRewardPool.Count > 0)
        {
            // UI: show victory with “Choosing cards…” state
            ui.ShowVictory(ga.Gold, ga.HealAmount, pickingCards: true);

            // Let player pick N cards from pool
            bool done = false;
            DeckChoiceSystem.Instance?.ChooseToAdd(ga.CardRewardPool, ga.PickCardCount, added =>
            {
                PlayerSystem.Instance?.AddCardsToDeckAndDrawPile(added, shuffleDrawPile: true);
                done = true;
            });
            yield return new WaitUntil(() => done);

            // Now enable the Next button
            ui.ShowCardChoices(null, 3, () => {
                ui.Hide();
                RunManager.Instance?.NextFloor();
            });
        }
        else
        {
            // No card picks — just show summary + Next
            ui.ShowVictory(ga.Gold, ga.HealAmount, pickingCards: false);
            ui.ShowCardChoices(null, 3, () => {
                ui.Hide();
                RunManager.Instance?.NextFloor();
            });
        }
    }

    private IEnumerator DefeatPerformer(CombatDefeatGA _)
    {
        TurnSystem.Instance?.SuspendCombat();

        yield return new WaitForSeconds(0.25f);

        var ui = CombatEndUI.Instance;
        if (ui != null)
        {
            ui.ShowDefeat(() =>
            {
                ui.Hide();
                // You can restart run, go to main menu, etc.
                Debug.Log("[CombatEndSystem] Defeat — returning to lobby/start.");
                RunManager.Instance?.StartRun(); // or a GameOver screen
            });
        }
        else
        {
            Debug.LogWarning("[CombatEndSystem] No CombatEndUI in scene — restarting run.");
            RunManager.Instance?.StartRun();
        }
    }
}
