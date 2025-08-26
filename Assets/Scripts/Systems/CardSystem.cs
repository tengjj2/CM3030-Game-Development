using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
[SerializeField] private HandView handView;
[SerializeField] private Transform drawPilePoint;
[SerializeField] private Transform discardPilePoint;
[SerializeField] private Transform exhaustPilePoint; // optional; falls back to discardPilePoint if null

[SerializeField] private List<CardData> runDeckData = new(); // the master deck for the run
public IReadOnlyList<CardData> RunDeckDataRO => runDeckData;

private readonly List<Card> drawPile = new();
private readonly List<Card> discardPile = new();
private readonly List<Card> exhaustPile = new(); // keeps Exhausted cards for THIS combat only
private readonly List<Card> hand        = new();

public IReadOnlyList<Card> DrawPileRO   => drawPile;
public IReadOnlyList<Card> DiscardPileRO => discardPile;
public IReadOnlyList<Card> HandRO        => hand;

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
        runDeckData = new List<CardData>(deckData ?? new List<CardData>());
        foreach (var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }
    public List<CardData> GetRunDeckData()
    {
        return runDeckData;
    }

    public void SetupRunDeck(List<CardData> startingDeck)
    {
        runDeckData = new List<CardData>(startingDeck ?? new List<CardData>());
        // you can also seed your in-combat piles here if you want
    }


    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardGA)
    {
        // Respect combat state so lobby doesn’t draw
        if (!TurnSystem.Instance || !TurnSystem.Instance.CombatActive) yield break;

        int want = Mathf.Max(0, drawCardGA.Amount);
        int canDraw = Mathf.Min(want, drawPile.Count);
        int remainder = want - canDraw;

        for (int i = 0; i < canDraw; i++)
            yield return DrawOne();

        if (remainder > 0)
        {
            RefillDeck(); // only from DISCARD (not EXHAUST)
            for (int i = 0; i < remainder && drawPile.Count > 0; i++)
                yield return DrawOne();
        }
    }

private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA _)
{
    if (!TurnSystem.Instance || !TurnSystem.Instance.CombatActive) yield break;

    // End-of-turn discard → to DISCARD pile (not exhaust)
    foreach (var card in hand)
    {
        CardView cv = handView.RemoveCard(card);
        yield return SendCardToPile(cv, Pile.Discard);
    }
    hand.Clear();
}

private IEnumerator PlayCardPerformer(PlayCardGA ga)
{
    // Remove from hand
    hand.Remove(ga.Card);
    CardView cardView = handView.RemoveCard(ga.Card);

    // Route to correct pile (Exhaust or Discard) based on the card’s flag
    bool toExhaust = ga.Card.ExhaustOnPlay; // assumes Card has this property
    yield return SendCardToPile(cardView, toExhaust ? Pile.Exhaust : Pile.Discard);

    // Spend energy
    ActionSystem.Instance.AddReaction(new SpendCostGA(ga.Card.Cost));

    // Manual-target effect
    if (ga.Card.ManualTargetEffect != null)
    {
        var caster = PlayerSystem.Instance.PlayerView;
        var pe = new PerformEffectGA(
            ga.Card.ManualTargetEffect,
            new() { ga.ManualTarget },
            caster
        );
        ActionSystem.Instance.AddReaction(pe);
    }

    // Other effects
    foreach (var effectWrapper in ga.Card.OtherEffects)
    {
        var caster = PlayerSystem.Instance.PlayerView;
        List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
        var pe = new PerformEffectGA(effectWrapper.Effect, targets, caster);
        ActionSystem.Instance.AddReaction(pe);
    }
}

    // ---------------- Internals ----------------


    public void RebuildFromRunDeck(List<CardData> deckData)
    {
        if (deckData == null || deckData.Count == 0)
        {
            Debug.LogError("[CardSystem] RebuildFromRunDeck called with empty/null deck. Aborting rebuild to avoid clearing everything.");
            return;
        }

        // 1) Destroy any hand views
        if (handView != null)
        {
            var snapshot = new List<Card>(hand);
            foreach (var c in snapshot)
            {
                var cv = handView.RemoveCard(c);
                if (cv) Destroy(cv.gameObject);
            }
        }

        // 2) Clear piles
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();
        hand.Clear();

        // 3) Rebuild
        foreach (var cd in deckData)
            drawPile.Add(new Card(cd));

        // 4) Shuffle
        Shuffle(drawPile);
    }

    public void RebuildForNewCombatFromRunDeck(IReadOnlyList<CardData> runDeck, bool shuffle = true)
    {
        // 1) Kill any hand views cleanly
        if (handView != null)
        {
            var handSnapshot = new List<Card>(hand);
            foreach (var c in handSnapshot)
            {
                var cv = handView.RemoveCard(c);
                if (cv != null && !cv.Equals(null)) Destroy(cv.gameObject);
            }
        }

        // 2) Clear all runtime piles
        hand.Clear();
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        // 3) Rebuild draw from run deck
        if (runDeck != null)
        {
            foreach (var cd in runDeck)
                if (cd != null) drawPile.Add(new Card(cd));
        }

        // 4) Shuffle so the new card can actually appear
        if (shuffle) Shuffle(drawPile);
    }


    public bool RemoveOneLiveInstanceByName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName)) return false;

        // Hand
        int hi = hand.FindIndex(c => c != null && c.Name == cardName);
        if (hi >= 0)
        {
            var card = hand[hi];
            hand.RemoveAt(hi);
            var cv = handView?.RemoveCard(card);
            if (cv) Destroy(cv.gameObject);
            return true;
        }

        // Draw
        int i = drawPile.FindIndex(c => c != null && c.Name == cardName);
        if (i >= 0) { drawPile.RemoveAt(i); return true; }

        // Discard
        i = discardPile.FindIndex(c => c != null && c.Name == cardName);
        if (i >= 0) { discardPile.RemoveAt(i); return true; }

        // Exhaust
        i = exhaustPile.FindIndex(c => c != null && c.Name == cardName);
        if (i >= 0) { exhaustPile.RemoveAt(i); return true; }

        return false;
    }

    public void AddCardDataToRunDeck(CardData data, bool alsoAddToDrawPile = true, bool toTop = false)
    {
        if (!data) return;
        runDeckData.Add(data);

        // reflect immediately if we're NOT in combat
        if (!TurnSystem.Instance || !TurnSystem.Instance.CombatActive)
        {
            if (alsoAddToDrawPile)
            {
                var c = new Card(data);
                if (toTop) drawPile.Insert(0, c);
                else drawPile.Add(c);
                Shuffle(drawPile);
            }
        }
    }

    // Remove ONE copy of a CardData from the run deck (no live-instance work).
    // If not in combat, rebuild piles to reflect the change immediately.
    public bool RemoveOneFromRunDeck(CardData data)
    {
        if (data == null || runDeckData == null) return false;
        int idx = runDeckData.IndexOf(data);
        if (idx >= 0) { runDeckData.RemoveAt(idx); return true; }
        // If instances differ (duplicate by value), fallback to name match:
        idx = runDeckData.FindIndex(d => d && d.Name == data.Name);
        if (idx >= 0) { runDeckData.RemoveAt(idx); return true; }
        return false;
    }

    private IEnumerator DrawOne()
    {
        if (drawPile.Count == 0) yield break;

        Card card = drawPile.Draw();
        hand.Add(card);

        CardView cv = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cv);
    }

// Only DISCARD refills the draw pile during combat.
private void RefillDeck()
{
    if (discardPile.Count == 0) return;
    drawPile.AddRange(discardPile);
    discardPile.Clear();
    Shuffle(drawPile);
}

private enum Pile { Discard, Exhaust }

public List<Card> GetFullDeck(
    bool includeDraw    = true,
    bool includeDiscard = true,
    bool includeExhaust = true,
    bool includeHand    = true)
{
    var result = new List<Card>(64);

    if (includeDraw)    result.AddRange(drawPile);
    if (includeDiscard) result.AddRange(discardPile);
    if (includeExhaust) result.AddRange(exhaustPile);
    if (includeHand)    result.AddRange(hand);

    return result;
}

private IEnumerator SendCardToPile(CardView cv, Pile pile)
{
    if (cv == null || cv.Equals(null)) yield break;

    // Hide hover mirror (prevents lingering scale tweens)
    CardViewHoverSystem.Instance?.Hide();

    // Kill ALL tweens on this card hierarchy before anim/destroy
    cv.transform.KillTweensRecursive();

    // Record to logic pile
    switch (pile)
    {
        case Pile.Discard: discardPile.Add(cv.Card); break;
        case Pile.Exhaust: exhaustPile.Add(cv.Card); break;
    }

    // Visual drop point (use discard if exhaust target is not set)
    Transform target = (pile == Pile.Exhaust && exhaustPilePoint != null) ? exhaustPilePoint : discardPilePoint;

    // Animate out & destroy
    var t = cv.transform;
    t.DOScale(Vector3.zero, 0.15f);
    var tween = t.DOMove(target.position, 0.15f);
    yield return tween.WaitForCompletion();

    t.KillTweensRecursive();
    Destroy(cv.gameObject);
}

// ---------------- Public helpers ----------------

public IReadOnlyList<Card> HandReadOnly => hand;

public IEnumerator DiscardFromHand(Card card)
{
    if (!hand.Contains(card)) yield break;
    hand.Remove(card);
    CardView cv = handView.RemoveCard(card);
    yield return SendCardToPile(cv, Pile.Discard);
}

public void RemoveFromDeck(Card card)
{
    drawPile.Remove(card);
    discardPile.Remove(card);
    exhaustPile.Remove(card);
    hand.Remove(card);
}

public void AddCardToDeck(CardData data, bool toTop = false)
{
    if (data == null) return;
    var newCard = new Card(data);
    if (toTop) drawPile.Insert(0, newCard);
    else       drawPile.Add(newCard);
}

public void AddCardsToDeck(List<CardData> datas, bool shuffleAfter = false)
{
    if (datas == null || datas.Count == 0) return;
    foreach (var cd in datas) AddCardToDeck(cd);
    if (shuffleAfter) Shuffle(drawPile);
}

private void Shuffle<T>(List<T> list)
{
    if (list == null || list.Count <= 1) return;
    for (int i = 0; i < list.Count; i++)
    {
        int j = Random.Range(i, list.Count);
        (list[i], list[j]) = (list[j], list[i]);
    }
}

// ------------- NEW: Hard reset for next combat -------------

/// <summary>
/// Fully reset piles for the NEXT combat:
/// - Destroys any lingering CardViews from the hand
/// - Moves DISCARD + EXHAUST back into DRAW
/// - Clears hand/discard/exhaust
/// - Shuffles draw pile
/// </summary>
public void ResetForNextCombat()
{
    // 1) Destroy any hand CardViews and clear hand list
    var handSnapshot = new List<Card>(hand);
    foreach (var c in handSnapshot)
    {
        var cv = handView.RemoveCard(c);
        if (cv != null && !cv.Equals(null)) Destroy(cv.gameObject);
    }
    hand.Clear();

    // 2) Move DISCARD and EXHAUST back to DRAW
    drawPile.AddRange(discardPile);
    discardPile.Clear();

    drawPile.AddRange(exhaustPile);
    exhaustPile.Clear();

    // 3) Shuffle draw
    Shuffle(drawPile);

    // (Optional) Any UI like cost/hand will be updated by TurnSystem at next turn start
}

/// <summary>Legacy alias if you already call this name elsewhere.</summary>
public void PrepareNewCombat() => ResetForNextCombat();

}
