using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Add Cards From Pool")]
public class AddCardsFromPoolEffect : LobbyEffectSO
{
    public List<CardData> Pool;
    public int Count = 2;

    public override void Apply(System.Action onComplete)
    {
        // No pool? Just finish.
        if (Pool == null || Pool.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Let the player choose "Count" cards from Pool, then add them to the deck via CardSystem.
        DeckChoiceSystem.Instance?.ChooseToAdd(Pool, Count, added =>
        {
            if (added != null && added.Count > 0 && CardSystem.Instance != null)
            {
                CardSystem.Instance.AddCardsToDeck(added, shuffleAfter:true);
            }
            onComplete?.Invoke();
        });
    }
}
