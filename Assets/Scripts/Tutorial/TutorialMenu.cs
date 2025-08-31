using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialMenu : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSection
    {
        public string sectionName;            // e.g., "Goal of Game"
        [TextArea] public string description; // section explanation text

        public Sprite sectionImage;

        // for "Card Effects" section: multiple small sprites and their descriptions
        public List<Sprite> cardSprites;
        public List<string> cardNames;
        public List<string> cardDescriptions;
    }

    [Header("Tutorial Data")]
    public List<TutorialSection> tutorialSections; // 5 sections: Goal, Lobby, Combat UI, Reward, Card Effects

    [Header("UI References")]
    public GameObject buttonPrefab;      // prefab for buttons in the options panel
    public Transform optionsPanel;       // panel where the buttons appear
    public GameObject descriptionScrollView;
    public Image displayImage;           // single large image for normal sections
    public TMP_Text displayDescription;  // description for normal sections

    [Header("Card Effects UI (for last section)")]
    public Transform cardEffectsContent; // parent object where small card images/descriptions will appear
    public GameObject cardEffectPrefab;  // prefab containing small card image + description

    [Header("Card Effects ScrollView")]
    public GameObject cardEffectsScrollView; // the parent scroll view for card effects


    [Header("Optional")]
    public int defaultSectionIndex = 0; // index of section to show first

    void Start()
    {
        GenerateButtons();
        gameObject.SetActive(false); // hide tutorial panel initially
    }

    void GenerateButtons()
    {
        // Clear existing buttons first
        foreach (Transform child in optionsPanel)
            Destroy(child.gameObject);

        // Create buttons for each tutorial section
        foreach (var section in tutorialSections)
        {
            GameObject newButton = Instantiate(buttonPrefab, optionsPanel);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = section.sectionName;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => ShowSection(section));
            }
        }
    }

    void ShowSection(TutorialSection section)
    {
        // Hide everything first
        if (displayImage != null)
            displayImage.gameObject.SetActive(false);
        if (displayDescription != null)
            displayDescription.gameObject.SetActive(false);
        if (descriptionScrollView != null)
            descriptionScrollView.SetActive(false);
        if (cardEffectsScrollView != null)
            cardEffectsScrollView.SetActive(false);  // NEW: hide the whole scrollview
        if (cardEffectsContent != null)
            cardEffectsContent.gameObject.SetActive(false);

        // Check if this is the "Card Effects" section
        bool isCardEffectsSection = section.cardSprites != null &&
                                    section.cardSprites.Count > 0 &&
                                    section.cardDescriptions != null &&
                                    section.cardSprites.Count == section.cardDescriptions.Count;

        if (isCardEffectsSection)
        {
            if (cardEffectsScrollView != null)
                cardEffectsScrollView.SetActive(true);  // show only for Card Effects

            if (cardEffectsContent != null && cardEffectPrefab != null)
            {
                // Clear previous entries
                foreach (Transform child in cardEffectsContent)
                    Destroy(child.gameObject);

                cardEffectsContent.gameObject.SetActive(true);

                // Instantiate new entries
                for (int i = 0; i < section.cardSprites.Count; i++)
                {
                    GameObject newEffect = Instantiate(cardEffectPrefab, cardEffectsContent);

                    // card image
                    Image img = newEffect.transform.Find("CardImage")?.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = section.cardSprites[i];
                        img.preserveAspect = true;
                    }

                    // card name
                    TMP_Text nameText = newEffect.transform.Find("TextContainer/CardName")?.GetComponent<TMP_Text>();
                    if (nameText != null)
                    {
                        if (section.cardNames != null && i < section.cardNames.Count && !string.IsNullOrEmpty(section.cardNames[i]))
                            nameText.text = section.cardNames[i];
                        else if (section.cardSprites[i] != null)
                            nameText.text = section.cardSprites[i].name;
                        else
                            nameText.text = $"Card {i + 1}";
                    }

                    // card description
                    TMP_Text descText = newEffect.transform.Find("TextContainer/CardDescription")?.GetComponent<TMP_Text>();
                    if (descText != null && section.cardDescriptions != null && i < section.cardDescriptions.Count)
                    {
                        descText.text = section.cardDescriptions[i];
                    }
                }

                // force layout update
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(cardEffectsContent.GetComponent<RectTransform>());

                // Reset Card Effects ScrollView to top
                ScrollRect cardScroll = cardEffectsScrollView?.GetComponent<ScrollRect>();
                if (cardScroll != null)
                    cardScroll.verticalNormalizedPosition = 1f;
            }
        }
        else
        {
            // Normal section
            if (descriptionScrollView != null)
                descriptionScrollView.SetActive(true);

            if (displayImage != null)
            {
                displayImage.sprite = section.sectionImage;
                displayImage.gameObject.SetActive(true);
            }

            if (displayDescription != null)
            {
                displayDescription.text = section.description;
                displayDescription.gameObject.SetActive(true);
            }

            // Reset main scroll
            ScrollRect scrollRect = descriptionScrollView?.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    // toggle the tutorial panel on/off
    public void TogglePanel(bool active)
    {
        gameObject.SetActive(active); // activate panel first

        if (active && tutorialSections.Count > 0)
        {
            // force layout rebuilds before showing content
            Canvas.ForceUpdateCanvases();

            // show first section immediately
            ShowSection(tutorialSections[defaultSectionIndex]);

            // if using a ScrollRect, reset scroll to top
            ScrollRect scrollRect = descriptionScrollView?.GetComponent<ScrollRect>();
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}