using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Remove Card From Run Deck")]
public class RemoveCardFromRunDeckEffect : LobbyEffectSO
{
    [Min(1)] public int Count = 1;                       // how many to remove
    [TextArea] public string Title  = "Remove a card";
    [TextArea] public string Prompt = "Choose a card to remove from your deck.";

    public override void Apply(System.Action onComplete)
    {
        // 1) Validate run deck
        var runDeck = PlayerSystem.Instance?.GetRunDeckData();
        if (runDeck == null || runDeck.Count == 0)
        {
            Debug.Log("[RemoveCardFromRunDeckEffect] Run deck empty or missing — nothing to remove.");
            onComplete?.Invoke();
            return;
        }

        // 2) Validate UI
        var panel = DeckChoicePanelUI.Instance;
        if (panel == null)
        {
            Debug.LogWarning("[RemoveCardFromRunDeckEffect] DeckChoicePanelUI.Instance not found. Skipping UI.");
            onComplete?.Invoke();
            return;
        }

        // 3) Open the picker (single pick by default; set Count > 1 to allow multi)
        //    DeckChoicePanelUI expects a List<CardData> pool and will call us back.
        panel.ShowChooseFromPool(
            pool: new List<CardData>(runDeck),
            countToPick: Mathf.Max(1, Count),
            title: Title,
            prompt: Prompt,
            multiSelect: Count > 1,
            onPicked: picked =>
            {
                // If player cancelled or nothing picked, still complete
                if (picked != null && picked.Count > 0)
                {
                    foreach (var cd in picked)
                        PlayerSystem.Instance?.RemoveCardDataFromRunDeck(cd);
                }
                onComplete?.Invoke();
            }
        );
    }
}
