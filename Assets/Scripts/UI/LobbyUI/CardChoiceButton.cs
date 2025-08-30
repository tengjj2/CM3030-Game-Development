using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardChoiceButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Image art;

    private System.Action onClick;

    public void Setup(CardData data, System.Action clicked)
    {
        onClick = clicked;
        if (nameText) nameText.text = data.Name;
        if (descText) descText.text = data.Description;
        if (art)      art.sprite = data.Image;

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}