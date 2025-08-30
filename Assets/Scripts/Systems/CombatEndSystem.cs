// CombatEndSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEndSystem : MonoBehaviour
{
    [SerializeField] private CombatEndUI endUI; // if you still use a summary/next UI
    [SerializeField] private CombatCardRewardPanelUI combatRewardPanel;
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
        TurnSystem.Instance?.SuspendCombat();
        yield return new WaitForSeconds(0.25f);

        // Gold from floor + GA
        var floor = RunManager.Instance?.CurrentFloor;
        int totalGold = Mathf.Max(0, floor?.GoldReward ?? 0) + Mathf.Max(0, ga?.Gold ?? 0);
        if (totalGold > 0) CurrencySystem.Instance?.AddGold(totalGold);

        // Show summary / paid heal if you still use CombatEndUI
        if (endUI)
        {
            // pickingCards shown true if we’re going to open the card picker
            bool hasPicks = (ga != null && ga.CardRewardPool != null && ga.CardRewardPool.Count > 0 && ga.PickCardCount > 0);
            endUI.ShowVictory(totalGold, 0, pickingCards: hasPicks);
            endUI.ShowPaidHeal(); 
            Debug.Log($"[CombatEnd] pool is {(ga.CardRewardPool == null ? "NULL" : "OK")}");
            Debug.Log($"[CombatEnd] pool.Count = {(ga.CardRewardPool != null ? ga.CardRewardPool.Count : -1)}");
            Debug.Log($"[CombatEnd] pickCount = {ga.PickCardCount}");
            Debug.Log($"[CombatEnd] hasPicks = {hasPicks}");
        }

        // Card choices (use the panel we serialized)
        if (ga != null && ga.CardRewardPool != null && ga.CardRewardPool.Count > 0 && ga.PickCardCount > 0)
        {
            bool done = false;
            combatRewardPanel.ShowChoices(
                pool: ga.CardRewardPool,
                countToPick: ga.PickCardCount,
                title: "Choose a card",
                prompt: "Pick one",
                onPicked: picks =>
                {
                    if (picks != null && picks.Count > 0)
                    {
                        foreach (var cd in picks)
                        {
                            // Use your run-deck add method; fallback to AddCardToDeck if needed
                            if (CardSystem.Instance)
                            {
                                CardSystem.Instance.AddCardDataToRunDeck(cd, alsoAddToDrawPile: true);
                                Debug.Log($"[Victory] Added to run deck: {cd?.name}");
                            }
                            else
                            {
                                CardSystem.Instance?.AddCardToDeck(cd);
                                Debug.Log($"[Victory] Added to run deck: {cd?.name}");
                            }
                        }
                    }
                    done = true;
                }
            );
            yield return new WaitUntil(() => done);
        }

        // Advance
        endUI?.EnableNext(true, () =>
        {
            endUI.Hide();
            RunManager.Instance?.NextFloor();
        });

        // If you don't use CombatEndUI, just go next here instead:
        // RunManager.Instance?.NextFloor();
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
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            });
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
