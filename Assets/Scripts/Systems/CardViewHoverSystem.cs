using System;
using UnityEngine;
using DG.Tweening;  // add this

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    public void Show(Card card, Vector3 position)
    {
        if (cardViewHover == null || cardViewHover.Equals(null)) return;

        cardViewHover.transform.KillTweensRecursive(); // ensure a clean slate
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
        cardViewHover.ResetCardImageScale();
    }

    public void Hide()
    {
        if (cardViewHover == null || cardViewHover.Equals(null)) return;

        cardViewHover.transform.KillTweensRecursive();
        cardViewHover.gameObject.SetActive(false);
    }
}