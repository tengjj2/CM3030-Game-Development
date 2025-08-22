using System;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Name => data.Name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public Effect ManualTargetEffect => data.ManualTargetEffect;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public int Cost { get; private set; }
    public bool ExhaustOnPlay;
    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        Cost = cardData.Cost;
        ExhaustOnPlay = data.ExhaustOnPlay;
    }

    void OnMouseDown()
    {
        if (ActionSystem.Instance.IsPerforming) return;
    }

}
