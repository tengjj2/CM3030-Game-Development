using UnityEngine;
using TMPro;
using DG.Tweening;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void OnEnable()
    {
        var cs = CurrencySystem.Instance;
        if (cs != null)
        {
            cs.OnGoldChanged -= HandleGoldChanged; // safety: avoid double-sub
            cs.OnGoldChanged += HandleGoldChanged;
            // Initial sync
            HandleGoldChanged(cs.Gold);
        }
        else
        {
            // If CurrencySystem spawns later, you could add a small bootstrapper or
            // call HandleGoldChanged(0) here.
            HandleGoldChanged(0);
        }
    }

    private void OnDisable()
    {
        var cs = CurrencySystem.Instance;
        if (cs != null) cs.OnGoldChanged -= HandleGoldChanged;
    }

    private void HandleGoldChanged(int gold) => Set(gold);

    public void Set(int gold)
    {
        if (!goldText) return;
        goldText.text = gold.ToString();

        // tiny feedback
        goldText.transform.DOKill();
        goldText.transform.localScale = Vector3.one;
        goldText.transform.DOScale(1.08f, 0.08f)
            .OnComplete(() => goldText.transform.DOScale(1f, 0.08f));
    }
}
