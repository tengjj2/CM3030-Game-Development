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
    public Image displayImage;           // single large image for normal sections
    public TMP_Text displayDescription;  // description for normal sections

    [Header("Card Effects UI (for last section)")]
    public Transform cardEffectsContent; // parent object where small card images/descriptions will appear
    public GameObject cardEffectPrefab;  // prefab containing small card image + description

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
        // hide everything first
        if (displayImage != null)
            displayImage.gameObject.SetActive(false);
        if (displayDescription != null)
            displayDescription.gameObject.SetActive(false);
        if (cardEffectsContent != null)
            cardEffectsContent.gameObject.SetActive(false);

        // check if Card Effects section
        bool isCardEffectsSection = section.cardSprites != null &&
                                    section.cardSprites.Count > 0 &&
                                    section.cardDescriptions != null &&
                                    section.cardSprites.Count == section.cardDescriptions.Count;

        if (isCardEffectsSection)
        {
            // show card effects content
            if (cardEffectsContent != null && cardEffectPrefab != null)
            {
                // clear previous entries
                foreach (Transform child in cardEffectsContent)
                    Destroy(child.gameObject);

                cardEffectsContent.gameObject.SetActive(true);

                for (int i = 0; i < section.cardSprites.Count; i++)
                {
                    GameObject newEffect = Instantiate(cardEffectPrefab, cardEffectsContent);

                    // set image
                    Image img = newEffect.transform.Find("CardImage")?.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = section.cardSprites[i];
                        img.preserveAspect = true;
                    }

                    // set name
                    TMP_Text nameText = newEffect.transform.Find("TextContainer/CardName")?.GetComponent<TMP_Text>();
                    if (nameText != null)
                    {
                        if (section.cardNames != null && i < section.cardNames.Count && !string.IsNullOrEmpty(section.cardNames[i]))
                            nameText.text = section.cardNames[i]; // use custom name
                        else if (section.cardSprites[i] != null)
                            nameText.text = section.cardSprites[i].name; // fallback to sprite name
                        else
                            nameText.text = $"Card {i + 1}";
                    }

                    // set description
                    TMP_Text desc = newEffect.transform.Find("TextContainer/CardDescription")?.GetComponent<TMP_Text>();
                    if (desc != null)
                        desc.text = section.cardDescriptions[i];
                }
            }
        }
        else
        {
            // normal section: show main image and description
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
        }
    }

    // toggle the tutorial panel on/off
    public void TogglePanel(bool active)
    {
        gameObject.SetActive(active);

        if (active && tutorialSections.Count > 0)
        {
            // show first section by default
            ShowSection(tutorialSections[defaultSectionIndex]);
        }
    }
}
