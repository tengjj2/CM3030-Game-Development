using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class TurnBannerUI : Singleton<TurnBannerUI>
{
    [Header("Assign in Inspector")]
    [SerializeField] private TMP_Text label;

    [Tooltip("If set, this RectTransform is animated; otherwise this GameObject's RectTransform is animated.")]
    [SerializeField] private RectTransform wrapper;

    [Header("Timing")]
    [SerializeField] private float inDuration = 0.5f;
    [SerializeField] private float holdDuration = 1.0f;
    [SerializeField] private float outDuration = 0.5f;
    [SerializeField] private float edgePadding = 64f;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    private RectTransform rt;        // animated rect
    private RectTransform parentRT;  // container
    private CanvasGroup cg;
    private Canvas rootCanvas;
    private Sequence playing;

    // IMPORTANT: override + call base.Awake()
    protected override void Awake()
    {
        base.Awake(); // ensures Singleton<T>.Instance is set

        cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var self = GetComponent<RectTransform>();
        rt = wrapper != null ? wrapper : self;

        // Force center anchors/pivot so "center" truly means screen center
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        parentRT = rt.parent as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>();

        gameObject.SetActive(true); // must be active for DOTween to run
    }

    private void OnDisable() { playing?.Kill(); }
    private void OnDestroy() { playing?.Kill(); }

    public IEnumerator ShowPlayerTurn() => Play("Player Turn");
    public IEnumerator ShowEnemyTurn()  => Play("Enemy Turn");

    public IEnumerator Play(string text)
    {
        if (label == null)
        {
            Debug.LogWarning("[TurnBannerUI] Label not assigned.");
            yield break;
        }

        // Ensure rect sizes are valid before we compute positions
        Canvas.ForceUpdateCanvases();
        (parentRT ?? rt.parent as RectTransform)?.ForceUpdateRectTransforms();
        rt.ForceUpdateRectTransforms();

        bool world = rootCanvas && rootCanvas.renderMode == RenderMode.WorldSpace;

        // Compute offscreen/center positions
        Vector2 leftOff, center, rightOff;

        if (!world)
        {
            float parentW = parentRT ? parentRT.rect.width : Screen.width;
            float myW     = rt.rect.width > 0 ? rt.rect.width : parentW * 0.5f;

            leftOff  = new Vector2(-parentW * 0.5f - myW * 0.5f - edgePadding, 0f);
            center   = Vector2.zero;
            rightOff = new Vector2( parentW * 0.5f + myW * 0.5f + edgePadding, 0f);

            rt.anchoredPosition = leftOff;
        }
        else
        {
            float parentW = parentRT ? parentRT.rect.width : 1920f;
            float myW     = rt.rect.width > 0 ? rt.rect.width : parentW * 0.5f;

            leftOff  = new Vector2(-(parentW * 0.5f + myW * 0.5f + edgePadding), 0f);
            center   = Vector2.zero;
            rightOff = new Vector2(  (parentW * 0.5f + myW * 0.5f + edgePadding), 0f);

            rt.localPosition = new Vector3(leftOff.x, leftOff.y, rt.localPosition.z);
        }

        // Set text + show
        label.text = text;
        cg.alpha = 1f;

        playing?.Kill();

        if (!world)
        {
            playing = DOTween.Sequence()
                .Append(rt.DOAnchorPos(center, inDuration).SetEase(Ease.OutCubic))
                .AppendInterval(holdDuration)
                .Append(rt.DOAnchorPos(rightOff, outDuration).SetEase(Ease.InCubic))
                .OnComplete(() =>
                {
                    cg.alpha = 0f;
                    rt.anchoredPosition = leftOff;
                });
        }
        else
        {
            Vector3 left3   = new Vector3(leftOff.x,  leftOff.y,  rt.localPosition.z);
            Vector3 center3 = new Vector3(center.x,   center.y,   rt.localPosition.z);
            Vector3 right3  = new Vector3(rightOff.x, rightOff.y, rt.localPosition.z);

            playing = DOTween.Sequence()
                .Append(rt.DOLocalMove(center3, inDuration).SetEase(Ease.OutCubic))
                .AppendInterval(holdDuration)
                .Append(rt.DOLocalMove(right3, outDuration).SetEase(Ease.InCubic))
                .OnComplete(() =>
                {
                    cg.alpha = 0f;
                    rt.localPosition = left3;
                });
        }

        if (verbose)
        {
            string mode = world ? "WORLD" : "SCREEN";
            Debug.Log($"[TurnBannerUI] Play '{text}' ({mode}) on {(wrapper ? wrapper.name : rt.name)}");
        }

        yield return playing.WaitForCompletion();
    }
}