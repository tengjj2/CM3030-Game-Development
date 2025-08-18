using UnityEngine.Splines;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    //[SerializeField] private float cardSpacing = 0.1f; // 0..1 param distance along the spline
    //[SerializeField] private float zStackOffset = 0.01f;
    [SerializeField] private float tweenDuration = 0.15f;

    public static HandView Instance { get; private set; }

    private List<CardView> cards = new();

    private void Awake()
    {
        Instance = this;
    }

    private bool selecting = false;
    private int needCount = 0;
    private string currentPrompt = "";
    private List<Card> chosenBuffer;
    private HashSet<CardView> selectable = new();

    // Call by DiscardChoiceSystem
    public IEnumerator SelectCardsFromHand(int count, List<Card> outChosen, string prompt = "")
    {
        if (count <= 0) yield break;
        selecting = true;
        needCount = count;
        currentPrompt = prompt;
        chosenBuffer = new List<Card>();

        // Enable selection visuals + click handlers
        foreach (var cv in cards)
        {
            if (cv == null) continue;
            selectable.Add(cv);
            cv.SetSelectionEnabled(true, OnCardClickedWhileSelecting);
        }

        // Optional: show your own prompt UI here if you have one
        Debug.Log($"[HandView] {currentPrompt}");

        // Wait until player has chosen enough
        while (selecting)
            yield return null;

        // Copy out the selection
        outChosen.Clear();
        outChosen.AddRange(chosenBuffer);
        chosenBuffer = null;
        currentPrompt = "";

        // Clean up
        foreach (var cv in selectable)
        {
            if (cv == null || cv.Equals(null)) continue;
            cv.SetSelectionEnabled(false, null);
            // ensure no leftover pulse/hover tweens
            cv.transform.KillTweensRecursive();
        }
        selectable.Clear();
    }

    private void OnCardClickedWhileSelecting(CardView cv)
    {
        if (!selecting || cv == null || !selectable.Contains(cv)) return;

        // Toggle selection or just accept one-shot picks.
        // For discard we’ll do immediate pick until we have enough.
        if (!chosenBuffer.Contains(cv.Card))
            chosenBuffer.Add(cv.Card);

        // Visual feedback (optional)
        cv.PulseSelected();

        if (chosenBuffer.Count >= needCount)
            selecting = false;
    }

    public IEnumerator AddCard(CardView cardView)
    {
        if (cardView == null) yield break;
        cards.Add(cardView);
        yield return UpdateCardPositions(tweenDuration);
    }

    public CardView RemoveCard(Card card)
    {
        CardView cv = GetCardView(card);
        if (cv == null) return null;

        CardViewHoverSystem.Instance.Hide();

        cards.Remove(cv);
        StartCoroutine(UpdateCardPositions(tweenDuration));
        return cv;
    }

    private CardView GetCardView(Card card)
    {
        return cards.FirstOrDefault(cv => cv.Card == card);
    }

    private IEnumerator UpdateCardPositions(float duration)
    {

        if (cards.Count == 0) yield break;
        float cardSpacing = 1f / 10f;
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;

       for (int i = 0; i < cards.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 foward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, foward).normalized);
            cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration);
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
        yield return new WaitForSeconds(duration);
    }
}