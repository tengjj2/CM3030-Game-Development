using UnityEngine;
using TMPro;
using DG.Tweening;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    public void Set(int gold)
    {
        if (!goldText) return;
        goldText.text = gold.ToString();

        // tiny feedback
        goldText.transform.DOKill();
        goldText.transform.localScale = Vector3.one;
        goldText.transform.DOScale(1.08f, 0.08f).OnComplete(() =>
            goldText.transform.DOScale(1f, 0.08f));
    }
}
