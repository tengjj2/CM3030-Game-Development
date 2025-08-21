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
        // Always clear any stale UI before starting
        DiscardPromptUI.Instance?.HideImmediate();

        var hand = CardSystem.Instance.HandReadOnly;
        if (hand.Count == 0) yield break;

        int want = Mathf.Min(ga.Count, hand.Count);
        string msg = want == 1 ? "Choose a card to discard" : $"Choose {want} cards to discard";
        DiscardPromptUI.Instance?.Show(msg);

        var chosen = new List<Card>();
        yield return HandView.Instance.SelectCardsFromHand(want, chosen, prompt: msg);

        foreach (var card in chosen)
            yield return CardSystem.Instance.DiscardFromHand(card);

        DiscardPromptUI.Instance?.Hide();
    }
}
