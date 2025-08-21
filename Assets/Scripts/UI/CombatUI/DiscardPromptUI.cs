using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DiscardPromptUI : Singleton<DiscardPromptUI>
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;          // this object’s CanvasGroup
    [SerializeField] private TMP_Text promptText;     // message text
    [SerializeField] private Image dimImage;          // full-screen Image for dimming

    [Header("Tweens")]
    [SerializeField] private float fade = 0.15f;
    [SerializeField] private float dimAlpha = 0.6f;

    protected override void Awake()
    {
        base.Awake();
        if (!cg) cg = GetComponent<CanvasGroup>();

        // We want clicks to pass through to cards.
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Start hidden
        cg.alpha = 0f;
        if (dimImage)
        {
            var c = dimImage.color;
            dimImage.color = new Color(c.r, c.g, c.b, 0f);
            // IMPORTANT: ensure dimImage.raycastTarget = false (so it doesn’t block clicks)
            dimImage.raycastTarget = false;
        }

        // Keep this object active, just hidden by alpha (so we can tween it anytime).
        gameObject.SetActive(true);
        Hide();
    }

    public void HideImmediate()
    {
        cg.DOKill();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Show(string msg)
    {
        if (promptText) promptText.text = msg;

        // Ensure this sits behind the cards but above the world:
        // - Put DiscardPromptUI under a UI canvas with sorting order below/behind your HandView canvas
        //   OR place it earlier in hierarchy so HandView renders after it.
        // - Keep dimImage.raycastTarget = false so clicks reach cards.

        cg.DOKill();
        cg.DOFade(1f, fade);

        if (dimImage)
        {
            dimImage.DOKill();
            var c = dimImage.color;
            dimImage.color = new Color(c.r, c.g, c.b, 0f);
            dimImage.DOFade(dimAlpha, fade);
        }
    }

    public void Hide()
    {
        cg.DOKill();
        cg.DOFade(0f, fade);

        if (dimImage)
        {
            dimImage.DOKill();
            dimImage.DOFade(0f, fade);
        }
    }
}
