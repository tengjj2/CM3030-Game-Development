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
        // Pause turns
        TurnSystem.Instance?.SuspendCombat();
        yield return new WaitForSeconds(0.25f);

        var rm = RunManager.Instance;
        Debug.Log($"[Victory] RunManager is {(rm ? "OK" : "NULL")}");

        var floor = rm?.CurrentFloor;
        Debug.Log($"[Victory] CurrentFloor is {(floor ? floor.name : "NULL")}");

        int floorGold = Mathf.Max(0, floor?.GoldReward ?? 0);
        int gaGold    = Mathf.Max(0, ga?.Gold ?? 0);
        int totalGold = floorGold + gaGold;

        Debug.Log($"[Victory] floorGold={floorGold}, gaGold={gaGold}, totalGold={totalGold}");

        // ---- 2) Apply to currency exactly once ----
        if (totalGold > 0)
        {
            Debug.Log($"[Currency] Before add: {CurrencySystem.Instance?.Gold}");
            CurrencySystem.Instance?.AddGold(totalGold);
            Debug.Log($"[Currency] After add: {CurrencySystem.Instance?.Gold}");
        }
        else
        {
            Debug.Log($"Total gold 0");
        }

        // ---- 3) Prepare UI ----
        var ui = CombatEndUI.Instance;
        if (ui == null)
        {
            Debug.LogWarning("[CombatEndSystem] No CombatEndUI in scene — advancing immediately.");
            RunManager.Instance?.NextFloor();
            yield break;
        }

        // Your “paid heal” flow (10 HP for 100g, multiple purchases)
        ui.ShowPaidHeal();  // keep this if you already wired it

        // Figure out if we’re doing card picks
        var pool = ga.CardRewardPool;
        int pickCount = Mathf.Max(0, ga.PickCardCount);
        bool hasPicks = (pool != null && pool.Count > 0 && pickCount > 0);

        // ---- 4) Show the summary ONCE with the totalGold we actually awarded ----
        ui.ShowVictory(gold: totalGold, heal: 0, pickingCards: hasPicks);

        if (hasPicks)
        {
            bool done = false;
            // Use your existing add flow
            DeckChoiceSystem.Instance?.ChooseToAdd(pool, pickCount, added =>
            {
                PlayerSystem.Instance?.AddCardsToDeckAndDrawPile(added, shuffleDrawPile: true);
                done = true;
            });
            yield return new WaitUntil(() => done);

            // After picks, enable "Next"
            ui.ShowCardChoices(null, 3, () =>
            {
                ui.Hide();
                RunManager.Instance?.NextFloor();
            });
        }
        else
        {
            // No picks – go straight to Next
            ui.ShowCardChoices(null, 3, () =>
            {
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
