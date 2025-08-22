// PlayerSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    [field: SerializeField] public PlayerView PlayerView { get; private set; }

    // Keep a reference to the active PlayerData so we can modify the persistent deck
    [SerializeField] private PlayerData playerData;

    public void Setup(PlayerData data)
    {
        playerData = data;
        PlayerView.Setup(data);
    }

    // Persistent deck reference (the list used to build runs)
    public List<CardData> PersistentDeck => playerData != null ? playerData.Deck : null;

    // Add gold (adjust to match your own gold model/UI)
    public void AddGold(int amount)
    {
        if (PlayerView != null)
        {
           //PlayerView.AddGold(amount); // if you have this
            // or store gold on PlayerSystem and update a UI here if that's your pattern
        }
    }

    /// Add cards to the persistent deck AND inject them into the current draw pile so they can show up this combat.
    public void AddCardsToDeckAndDrawPile(List<CardData> newCards, bool shuffleDrawPile = true)
    {
        if (newCards == null) return;

        if (playerData?.Deck != null)
            playerData.Deck.AddRange(newCards);

        // put them into the live draw pile now
        CardSystem.Instance?.AddCardsToDeck(newCards, shuffleDrawPile);
    }
}
