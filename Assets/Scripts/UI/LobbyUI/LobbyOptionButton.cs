// LobbyOptionButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    public void Setup(LobbyChoice choice, System.Action onClicked)
    {
        if (titleText) titleText.text = choice?.Label ?? "";
        if (descText)  descText.text  = choice?.Description ?? "";
        if (icon) {
            icon.sprite  = choice?.Icon;
            icon.enabled = (choice?.Icon != null);
        }

        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            AudioManager.Instance.Play("click_button");
            Debug.Log("[LobbyOptionButton] Clicked: " + (choice?.Label ?? "<null>"));
            onClicked?.Invoke();
        });
    }
}