using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Speaker & Text")]
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text continueLabel;

    [Header("Controls")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button nextFloorButton;

    [Header("Choices Area")]
    [SerializeField] private RectTransform optionsRoot;
    [SerializeField] private LobbyOptionButton optionButtonPrefab;

    [Header("Frame / Flip Settings")]
    [SerializeField] private RectTransform frameRect;           // wrapper (DO NOT flip this)
    [SerializeField] private Image         frameGraphicImage;   // <- flip this only
    [SerializeField] private RectTransform portraitRect;
    [SerializeField] private RectTransform nameRect;
    [SerializeField] private RectTransform bodyRect;
    [SerializeField] private RectTransform continueRect;
    [SerializeField] private RectTransform choicesRect;
    [SerializeField] private SpeakerSO receptionistSO;

    [SerializeField] private float typeDelay = 0.02f;   // seconds per character
    [SerializeField] private bool  autoFadeInOnShow = true;
    [SerializeField] private float fadeInDuration = 0.2f;
    private bool waitingForContinue = false;
    private bool isTyping = false;
    private Coroutine typingCo;

    // ---------- Baseline storage ----------
    private struct RectBaseline
    {
        public RectTransform rt;
        public Vector2 anchorMin, anchorMax, pivot, anchoredPos;
        public Vector3 localScale;
    }
    private readonly List<RectBaseline> baselines = new();

    // Keep baseline text alignments to mirror/restore
    private TextAlignmentOptions nameAlignBase;
    private TextAlignmentOptions bodyAlignBase;

    // Choice state
    private Action<int> onChoice;
    private readonly List<GameObject> spawned = new();

    private void Awake()
    {
        Debug.Log($"[DialoguePanelUI] Awake on {name} (id={GetInstanceID()})");
        gameObject.SetActive(false);

        // Capture baselines for everything we will mirror (do NOT capture frameRect for mirroring)
        CaptureBaseline(portraitRect);
        CaptureBaseline(nameRect);
        CaptureBaseline(bodyRect);
        CaptureBaseline(continueRect);
        CaptureBaseline(choicesRect);
        // Also capture the frameGraphicImage rect’s baseline so we can restore its scale
        if (frameGraphicImage && frameGraphicImage.rectTransform)
            CaptureBaseline(frameGraphicImage.rectTransform);

        if (nameText) nameAlignBase = nameText.alignment;
        if (bodyText) bodyAlignBase = bodyText.alignment;

        if (continueButton)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        ShowContinue(true);
        ShowChoices(false);
        SetupNextFloorButton();
    }

    public void ShowNextFloorButton(System.Action onClick, string label = "Next Floor")
    {
        // Ensure panel is visible & interactive
        gameObject.SetActive(true);
        var cg = GetComponent<CanvasGroup>();
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        // Hide choices so the button isn’t overlapped
        ShowChoices(false);

        // Hide the continue button so there’s no ambiguity
        ShowContinue(false);

        // Wire and show
        if (nextFloorButton)
        {
            nextFloorButton.onClick.RemoveAllListeners();
            nextFloorButton.onClick.AddListener(() => onClick?.Invoke());
            nextFloorButton.gameObject.SetActive(true);
            nextFloorButton.transform.SetAsLastSibling(); // bring to front, optional
        }
    }

    public void HideNextFloorButton()
    {
        if (nextFloorButton) nextFloorButton.gameObject.SetActive(false);
    }

    private void SetupNextFloorButton()
    {
        if (nextFloorButton)
        {
            nextFloorButton.onClick.RemoveAllListeners();
            nextFloorButton.gameObject.SetActive(false); // hidden by default
        }
    }
    private void CaptureBaseline(RectTransform rt)
    {
        if (!rt) return;
        baselines.Add(new RectBaseline
        {
            rt = rt,
            anchorMin = rt.anchorMin,
            anchorMax = rt.anchorMax,
            pivot = rt.pivot,
            anchoredPos = rt.anchoredPosition,
            localScale = rt.localScale
        });
    }

    // ---------- Public API ----------

    /// NPC speaks (shows portrait/name/text + a Continue button)
    public void ShowNextButton(string label = "Continue", System.Action onClick = null, string body = null)
    {
        // Make sure panel + CanvasGroup are visible and clickable
        gameObject.SetActive(true);
        var cg = GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // Hide choices UI if still lingering
        ShowChoices(false);

        // Update body text if provided
        if (body != null) SetBody(body);

        // Ensure the continue container is active
        if (continueRect && !continueRect.gameObject.activeSelf) continueRect.gameObject.SetActive(true);

        // Show the button
        ShowContinue(true);

        // Update the button label (prefer your serialized continueLabel if set)
        if (continueLabel) continueLabel.text = label;
        else
        {
            var txt = continueButton ? continueButton.GetComponentInChildren<TMPro.TMP_Text>(true) : null;
            if (txt) txt.text = label;
        }

        // Replace listeners
        if (continueButton)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => onClick?.Invoke());
        }

        // (Optional) Bring the continue button to front
        if (continueButton) continueButton.transform.SetAsLastSibling();
    }

    public IEnumerator ShowNpcSpeech(SpeakerSO npc, string text, bool flipFrame = false)
    {
        onChoice = null;          // not in choice mode
        ClearChoices();

        SetSpeaker(npc);
        SetBody("");              // type into this

        SetFrameFlip(flipFrame);

        ShowContinue(true);
        ShowChoices(false);

        // --- ACTIVATE FIRST so coroutines & button work ---
        gameObject.SetActive(true);

        // optional fade-in
        var cg = GetComponent<CanvasGroup>();
        if (cg && autoFadeInOnShow)
        {
            cg.alpha = 0f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.DOFade(1f, fadeInDuration);
        }
        else if (cg)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // wire continue: if clicked while typing, finish typing; otherwise advance
        waitingForContinue = true;
        if (continueButton)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                if (isTyping)
                {
                    // skip to full text
                    isTyping = false;
                }
                else
                {
                    waitingForContinue = false;
                }
            });
        }

        // run typewriter (now that the object is active)
        yield return StartCoroutine(TypeText(text));

        // wait for the player to press continue (if they didn’t skip while typing)
        while (waitingForContinue)
            yield return null;
    }

    

    public IEnumerator ShowNpcSpeechLines(SpeakerSO npc, string[] lines, bool flipFrame = false)
    {
        if (lines == null || lines.Length == 0) yield break;

        for (int i = 0; i < lines.Length; i++)
        {
            yield return ShowNpcSpeech(npc, lines[i], flipFrame);
        }
    }

    /// Flip to player + show boon options inside the box
    public void ShowPlayerChoices(SpeakerSO player, List<LobbyChoice> choices, System.Action<int> onPicked)
    {
        ClearChoices();              // clears any old spawned buttons (but DO NOT hide)
        SetSpeaker(player);
        SetBody("");
        ApplyFlip(false);

        ShowContinue(false);
        ShowChoices(true);

        if (choices != null && optionButtonPrefab && optionsRoot)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                var btn = Instantiate(optionButtonPrefab, optionsRoot);
                btn.Setup(choices[i], () =>
                {
                    Debug.Log($"[DialoguePanelUI] Button clicked idx={idx}");

                    // Collapse choice UI now (do NOT Hide the whole panel)
                    ClearChoicesUI();

                    try
                    {
                        onPicked?.Invoke(idx);  // <— call LobbySystem directly
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                });
                spawned.Add(btn.gameObject);
            }
        }

        gameObject.SetActive(true);
    }

    public void SetFrameFlip(bool flipped)
    {
        // flip the frame graphic only (mirrors the bubble)
        if (frameGraphicImage != null)
        {
            var t = frameGraphicImage.rectTransform;
            var s = t.localScale;
            s.x = Mathf.Abs(s.x) * (flipped ? -1f : 1f);
            t.localScale = s;
        }

        // also mirror our saved rects (portrait/name/text/continue) if you used baseline mirroring earlier
        ApplyFlip(flipped);
    }

    public void Hide()
    {
        ClearChoices();
        gameObject.SetActive(false);
    }
    

    // ---------- Internals ----------

    private void SetSpeaker(SpeakerSO s)
    {
        if (nameText) nameText.text = s ? s.DisplayName : "";
        if (portrait)
        {
            portrait.sprite = s ? s.Portrait : null;
            portrait.enabled = s && s.Portrait;
        }
    }

    private void SetBody(string text) { if (bodyText) bodyText.text = text ?? ""; }

    private void ShowContinue(bool v) { if (continueButton) continueButton.gameObject.SetActive(v); }
    private void ShowChoices(bool v)
    {
        if (optionsRoot) optionsRoot.gameObject.SetActive(v);
        if (choicesRect) choicesRect.gameObject.SetActive(v);
    }

    private void ClearChoices()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        onChoice = null;
    }

    private void OnContinueClicked() { /* replaced by ShowNpcSpeech */ }

    // Replace your Select in DialoguePanelUI
    private void Select(int index)
    {
        Debug.Log($"[DialoguePanelUI] Awake on {name} (id={GetInstanceID()})");
        Debug.Log("[DialoguePanelUI] Select index = " + index);
        var cb = onChoice;
        ClearChoicesUI();      // collapse/remove choice buttons
        // DO NOT call Hide() here
        cb?.Invoke(index);
    }

    public void ClearChoicesUI()
    {
        ShowChoices(false);
        ClearChoices(); // destroys spawned buttons
    }
    // ---------- Flip Helpers ----------

    /// Apply or clear horizontal mirroring for the content + flip only the frame graphic.
    private void ApplyFlip(bool flipped)
    {
        // 1) Restore all baselines first (ensures no cumulative distortion)
        foreach (var b in baselines)
        {
            if (!b.rt) continue;
            b.rt.anchorMin = b.anchorMin;
            b.rt.anchorMax = b.anchorMax;
            b.rt.pivot = b.pivot;
            b.rt.anchoredPosition = b.anchoredPos;
            b.rt.localScale = b.localScale;
        }
        if (nameText) nameText.alignment = nameAlignBase;
        if (bodyText) bodyText.alignment = bodyAlignBase;

        // 2) If flipping, mirror only the content rects (not the frame wrapper)
        if (flipped)
        {
            MirrorIfHasBaseline(portraitRect);
            MirrorIfHasBaseline(nameRect);
            MirrorIfHasBaseline(bodyRect);
            MirrorIfHasBaseline(continueRect);
            MirrorIfHasBaseline(choicesRect);

            if (nameText) nameText.alignment = MirrorAlignment(nameAlignBase);
            if (bodyText) bodyText.alignment = MirrorAlignment(bodyAlignBase);
        }

        // 3) Flip ONLY the frame graphic image by negative X localScale
        if (frameGraphicImage && frameGraphicImage.rectTransform)
        {
            var frt = frameGraphicImage.rectTransform;
            var s = frt.localScale;
            s.x = Mathf.Abs(s.x) * (flipped ? -1f : 1f);
            frt.localScale = s;
        }
    }

    private void MirrorIfHasBaseline(RectTransform rt)
    {
        if (!rt) return;
        // find its baseline
        for (int i = 0; i < baselines.Count; i++)
        {
            if (baselines[i].rt == rt)
            {
                MirrorHorizontally(rt, baselines[i]);
                return;
            }
        }
    }

    private void StopTypingIfAny()
    {
        if (typingCo != null) { StopCoroutine(typingCo); typingCo = null; }
    }

    private System.Collections.IEnumerator TypeText(string text)
    {
        if (!bodyText) yield break;
        isTyping = true;
        bodyText.text = "";
        foreach (char c in text)
        {
            bodyText.text += c;
            yield return new WaitForSeconds(typeDelay);
            // allow skip-typing if the user clicks Continue while typing
            if (!isTyping) break;
        }
        // if they skipped, ensure full text is shown
        bodyText.text = text;
        isTyping = false;
    }

    /// Mirror a RectTransform by flipping anchors, pivot, and anchoredPosition.x against its baseline.
    private void MirrorHorizontally(RectTransform rt, RectBaseline baseL)
    {
        Vector2 aMin = baseL.anchorMin; aMin.x = 1f - aMin.x;
        Vector2 aMax = baseL.anchorMax; aMax.x = 1f - aMax.x;
        Vector2 piv = baseL.pivot; piv.x = 1f - piv.x;

        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = piv;

        Vector2 pos = baseL.anchoredPos; pos.x = -pos.x;
        rt.anchoredPosition = pos;

        // Keep content scale positive (we’re not visual‑flipping children)
        rt.localScale = new Vector3(Mathf.Abs(baseL.localScale.x), baseL.localScale.y, baseL.localScale.z);
    }

    /// Swap Left/Right while preserving centered alignments.
    private TextAlignmentOptions MirrorAlignment(TextAlignmentOptions a)
    {
        switch (a)
        {
            case TextAlignmentOptions.TopLeft:     return TextAlignmentOptions.TopRight;
            case TextAlignmentOptions.Left:        return TextAlignmentOptions.Right;
            case TextAlignmentOptions.BottomLeft:  return TextAlignmentOptions.BottomRight;
            case TextAlignmentOptions.TopRight:    return TextAlignmentOptions.TopLeft;
            case TextAlignmentOptions.Right:       return TextAlignmentOptions.Left;
            case TextAlignmentOptions.BottomRight: return TextAlignmentOptions.BottomLeft;
            default:                               return a; // centered etc.
        }
    }
}
