using System.Collections.Generic;
using UnityEngine;

public class DeckChoiceSystem : Singleton<DeckChoiceSystem>
{
    [SerializeField] private DeckChoiceUIController ui;

    public void ChooseToAdd(List<CardData> pool, int count, System.Action<List<CardData>> onDone)
    {
        ui?.ShowAdd("Choose cards to add", pool, count, onDone);
    }

    public void ChooseToRemove(List<CardData> deck, int count, System.Action<List<CardData>> onDone)
    {
        ui?.ShowRemove("Choose cards to remove", deck, count, onDone);
    }
}