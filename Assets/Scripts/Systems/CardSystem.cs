using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
    }

    public void Setup(List<CardData> deckData)
    {
        foreach (var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }

    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardGA)
    {
        int actualAmount = Mathf.Min(drawCardGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardGA.Amount - actualAmount;
        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }
        if (notDrawnAmount > 0)
        {
            RefillDeck();
            for (int i = 0; i < notDrawnAmount; i++)
            {
                yield return DrawCard();
            }
        }
    }


    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);
        SpendCostGA spendCostGA = new(playCardGA.Card.Cost);
        ActionSystem.Instance.AddReaction(spendCostGA);
        // Manual-target effect
        if (playCardGA.Card.ManualTargetEffect != null)
        {
            var caster = PlayerSystem.Instance.PlayerView;
            var pe = new PerformEffectGA(playCardGA.Card.ManualTargetEffect,
                                        new() { playCardGA.ManualTarget },
                                        caster);                        // pass caster
            ActionSystem.Instance.AddReaction(pe);
        }

        // Other effects
        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            var caster = PlayerSystem.Instance.PlayerView;
            List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
            var pe = new PerformEffectGA(effectWrapper.Effect, targets, caster); // pass caster
            ActionSystem.Instance.AddReaction(pe);
        }
    }

    //Reactions
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        if (cardView == null || cardView.Equals(null)) yield break;

        // Hide hover mirror so it can’t keep a scale tween alive
        if (CardViewHoverSystem.Instance != null)
            CardViewHoverSystem.Instance.Hide();

        // Kill ALL tweens on this card and children before animating/destroying
        cardView.transform.KillTweensRecursive();

        discardPile.Add(cardView.Card);

        // Animate out (these are new tweens on the same transform)
        var t = cardView.transform;
        t.DOScale(Vector3.zero, 0.15f);
        var tween = t.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();

        // Kill again just in case any late tweens were started (e.g., selection ping)
        t.KillTweensRecursive();
        Destroy(cardView.gameObject);
    }

    public IReadOnlyList<Card> HandReadOnly => hand;

    // Discard a specific card that is currently in hand.
    public IEnumerator DiscardFromHand(Card card)
    {
        if (!hand.Contains(card)) yield break;
        hand.Remove(card);
        CardView cardView = handView.RemoveCard(card);
        yield return DiscardCard(cardView); // you already have this private method
}
}