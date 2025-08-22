using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Add Cards (Rewards Panel)")]
public class LobbyAddCardsEffect : LobbyEffectSO
{
    [Tooltip("If set, use this pool. Otherwise CardRewardPanelUI will use CardLibrarySO attached to it.")]
    public List<CardData> Pool;

    [Min(1)] public int CountToPick = 1;

    public override void Apply(System.Action onComplete)
    {
        // Open the generic picker (UICardView-based)
        CardRewardPanelUI.Instance?.ShowChoices(
            pool: Pool,
            countToPick: CountToPick,
            title: "Choose a card",
            prompt: CountToPick > 1 ? $"Pick {CountToPick} cards to add" : "Pick a card to add",
            onPicked: picked =>
            {
                if (picked != null && picked.Count > 0)
                {
                    // Add to the current run deck/draw pile (out of combat is fine)
                    CardSystem.Instance?.AddCardsToDeck(picked, shuffleAfter: true);
                }
                onComplete?.Invoke();
            }
        );
    }
}
