// PlayerSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    [field: SerializeField] public PlayerView PlayerView { get; private set; }

    // Keep a reference to the active PlayerData so we can modify the persistent deck
    [SerializeField] private PlayerData playerData;
    private readonly List<CardData> runDeck = new();

    public IReadOnlyList<CardData> GetRunDeckData() => runDeck;
    public void AddCardDataToRunDeck(CardData c) { if (c) runDeck.Add(c); }
    public void RemoveCardDataFromRunDeck(CardData c) { if (c) runDeck.Remove(c); }

    public void Setup(PlayerData data)
    {
        playerData = data;
        PlayerView.Setup(data);
    }

    // Persistent deck reference (the list used to build runs)
    public List<CardData> PersistentDeck => playerData != null ? playerData.Deck : null;
    public List<CardData> RunDeckData = new();

    // Add gold (adjust to match your own gold model/UI)
    public void AddGold(int amount)
    {
        if (PlayerView != null)
        {
        }
    }

    public void InitializeRun(PlayerData data)
    {
        PlayerData startingData = data;
        runDeck.Clear();
        if (startingData != null && startingData.Deck != null)
            runDeck.AddRange(startingData.Deck);

        // (re)init HP, gold, etc. here too if you want
        // PlayerView.MaxHealth = startingData.MaxHP; etc.
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
