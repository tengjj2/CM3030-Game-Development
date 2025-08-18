// DiscardChoiceSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardChoiceSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ForceDiscardGA>(Performer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ForceDiscardGA>();
    }

    private IEnumerator Performer(ForceDiscardGA ga)
    {
        // If no cards in hand, nothing to do
        var hand = CardSystem.Instance.HandReadOnly;
        if (hand.Count == 0) yield break;

        int want = Mathf.Min(ga.Count, hand.Count);
        // Ask the HandView to enter selection mode, await player choice(s)
        var chosen = new List<Card>();
        yield return HandView.Instance.SelectCardsFromHand(want, chosen, prompt: (want == 1 ? "Choose a card to discard" : $"Choose {want} cards to discard"));

        // Discard chosen
        foreach (var card in chosen)
            yield return CardSystem.Instance.DiscardFromHand(card);
    }
}