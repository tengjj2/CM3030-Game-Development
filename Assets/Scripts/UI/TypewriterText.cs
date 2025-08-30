using System.Collections;
using UnityEngine;
using TMPro; // import TMP namespace

public class TypewriterText : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI uiText;      // TMP text component
    public GameObject startButton;

    [Header("Typing Settings")]
    [TextArea(3, 10)]
    public string[] paragraphs = new string[]
    {
        "You came here for a job interview.But the moment you walk into the building, everything goes wrong.",
        "The fire alarm won’t stop ringing. Papers are flying everywhere.Someone set the break room on fire. The elevators are broken.",
        "And now people are fighting in the hallways like it’s part of the job.",
        "If you want that position on the top floor,you’ll have to climb this office one level at a time,",
        "deal with the chaos, and somehow still make it to your interview.",
        "",
        "Welcome to Office Rumble."
    };

    public float typingSpeed = 0.1f;

    private int currentParagraph = 0;
    private bool isTyping = false;
    private bool isWaiting = false;

    void Start()
    {
        uiText.text = "";
        startButton.SetActive(false);
        StartCoroutine(TypeText());
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            if (isTyping)
            {
                // Instantly finish the current paragraph
                StopAllCoroutines();
                uiText.text = paragraphs[currentParagraph];
                isTyping = false;
                isWaiting = true;
            }
            else if (isWaiting)
            {
                isWaiting = false;
                currentParagraph++;

                if (currentParagraph < paragraphs.Length)
                {
                    StartCoroutine(TypeText());
                }
                else
                {
                    startButton.SetActive(true);
                }
            }
        }
    }


    IEnumerator TypeText()
    {
        isTyping = true;
        uiText.text = "";

        string paragraph = paragraphs[currentParagraph];

        foreach (char c in paragraph)
        {
            uiText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        isWaiting = true;
    }
}
