using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialMenu : MonoBehaviour
{
    [System.Serializable]
    public class TutorialSection
    {
        public string sectionName;       // e.g., "Goal of Game"
        public Sprite sectionImage;      // optional image for the section
        [TextArea] public string description; // explanation text
    }

    [Header("Tutorial Data")]
    public List<TutorialSection> tutorialSections; // 5 sections: Goal, Lobby, Combat UI, Reward, Card Effects

    [Header("UI References")]
    public GameObject buttonPrefab;   // prefab for buttons in the options panel
    public Transform optionsPanel;    // panel where the buttons will appear
    public Image displayImage;        // panel image for selected section
    public TMP_Text displayText;      // panel text for selected section

    void Start()
    {
        GenerateButtons();       // create buttons for all tutorial sections
        gameObject.SetActive(false); // hide tutorial panel initially
    }

    void GenerateButtons()
    {
        // Clear existing buttons first (if any)
        foreach (Transform child in optionsPanel)
        {
            Destroy(child.gameObject);
        }

        // Create new buttons
        foreach (var section in tutorialSections)
        {
            GameObject newButton = Instantiate(buttonPrefab, optionsPanel);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = section.sectionName;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    ShowSection(section);
                });
            }
        }
    }

    void ShowSection(TutorialSection section)
    {
        displayText.text = section.description;

        if (section.sectionImage != null)
            displayImage.sprite = section.sectionImage;
        else
            displayImage.sprite = null; // hide image if none assigned
    }

    public void TogglePanel(bool active)
    {
        gameObject.SetActive(active);
    }
}
