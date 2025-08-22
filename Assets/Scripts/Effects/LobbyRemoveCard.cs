using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Remove Card From Deck")]
public class RemoveCardFromDeckEffect : LobbyEffectSO
{
    public int Count = 1;

    public override void Apply(System.Action onComplete)
    {
        var cardSystem = CardSystem.Instance;
        if (cardSystem != null)
        {
            // Get the full deck (could be draw + discard + hand, depending on design)
            var fullDeck = cardSystem.GetFullDeck(); // you'll add this method
            if (fullDeck.Count > 0)
            {
                // TODO: Show UI so player chooses one card to remove
                var chosen = fullDeck[0]; // placeholder
                cardSystem.RemoveFromDeck(chosen);
            }
        }
        onComplete?.Invoke();
    }
}