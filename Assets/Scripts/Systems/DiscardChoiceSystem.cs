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

    public static bool IsChoosing { get; private set; }
    public static event System.Action<bool> OnChoosingChanged;

    private void SetChoosing(bool v)
    {
        if (IsChoosing == v) return;
        IsChoosing = v;
        OnChoosingChanged?.Invoke(v);
    }

    private IEnumerator Performer(ForceDiscardGA ga)
    {
        var hand = CardSystem.Instance.HandReadOnly;
        if (hand.Count == 0) yield break;

        SetChoosing(true);
        int want = Mathf.Min(ga.Count, hand.Count);

        var chosen = new List<Card>();
        yield return HandView.Instance.SelectCardsFromHand(
            want, chosen,
            prompt: (want == 1 ? "Choose a card to discard" : $"Choose {want} cards to discard")
        );

        foreach (var card in chosen)
            yield return CardSystem.Instance.DiscardFromHand(card);

        SetChoosing(false);
    }
}
