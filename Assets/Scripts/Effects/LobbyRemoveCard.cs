using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Remove Card(s) From Deck")]
public class RemoveCardFromRunDeckEffect : LobbyEffectSO
{
    public int Count = 1;

    public override void Apply(Action onComplete)
    {
        var cs = CardSystem.Instance;
        if (cs == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Build pickable pool from current RUN DECK (CardData)
        var pool = new List<CardData>(cs.RunDeckDataRO);
        if (pool.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        DeckChoiceSystem.Instance.ChooseCardDatas(
            pool,
            Mathf.Clamp(Count, 1, pool.Count),
            chosen =>
            {
                if (chosen != null)
                {
                    foreach (var cd in chosen)
                        cs.RemoveOneFromRunDeck(cd); // updates piles immediately if out of combat
                }
                onComplete?.Invoke();
            },
            title: "Remove cards",
            prompt: (Count == 1 ? "Choose a card to remove" : $"Choose {Count} cards to remove")
        );
    }
}
