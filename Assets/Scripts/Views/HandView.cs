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

    private List<CardView> cards = new();

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