using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIController : MonoBehaviour
{
    [Serializable]
    public struct OptionVisual
    {
        public string Label;
        public Sprite Icon;
    }

    [Header("Top Section")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Option Buttons")]
    [SerializeField] private Button[] optionButtons;   // e.g., 3-4 buttons laid out
    [SerializeField] private TMP_Text[] optionLabels;  // label per button
    [SerializeField] private Image[] optionIcons;      // optional icon per button

    private Action<int> onChosen;

    private void Awake()
    {
        // Wire button clicks once
        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int idx = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => Choose(idx));
                }
            }
        }
        gameObject.SetActive(false);
    }

    // ---- Public API expected by LobbySystem --------------------------------

    /// <summary>
    /// Convenience wrapper: build visuals from offer and show.
    /// </summary>
    public void Open(LobbyOfferSO offer, Action<int> onSelect)
    {
        if (offer == null)
        {
            Debug.LogError("[LobbyUIController] Open called with null offer.");
            return;
        }

        // Build visuals from the offer options
        var visuals = new OptionVisual[offer.Options.Count];
        for (int i = 0; i < visuals.Length; i++)
        {
            var src = offer.Options[i];
            visuals[i] = new OptionVisual
            {
                Label = src.Label,
                Icon  = src.Icon
            };
        }

        Show(offer.Title, offer.Description, visuals, onSelect);
    }

    // Alias so other code can call Show(...) directly if it wants.
    public void Show(string title, string description, OptionVisual[] options, Action<int> onSelect)
        => ShowOffer(title, description, options, onSelect);

    public void Close() => Hide();

    // ---- Existing UI population logic --------------------------------------

    public void ShowOffer(string title, string description, OptionVisual[] options, Action<int> onChosen)
    {
        this.onChosen = onChosen;

        if (titleText)       titleText.text = title ?? "";
        if (descriptionText) descriptionText.text = description ?? "";

        // Fill visible options, disable unused buttons
        int count = optionButtons != null ? optionButtons.Length : 0;
        for (int i = 0; i < count; i++)
        {
            bool has = (options != null && i < options.Length);
            var btn = optionButtons[i];
            if (btn != null) btn.gameObject.SetActive(has);
            if (!has) continue;

            var ov = options[i];

            if (optionLabels != null && i < optionLabels.Length && optionLabels[i] != null)
                optionLabels[i].text = ov.Label ?? "";

            if (optionIcons != null && i < optionIcons.Length && optionIcons[i] != null)
            {
                optionIcons[i].sprite  = ov.Icon;
                optionIcons[i].enabled = ov.Icon != null;
            }
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onChosen = null;
    }

    private void Choose(int index)
    {
        var cb = onChosen;
        Hide();
        cb?.Invoke(index);
    }
}