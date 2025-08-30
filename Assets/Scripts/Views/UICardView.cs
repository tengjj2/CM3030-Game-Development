using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICardView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image    artImage;    // preserve aspect = ON
    [SerializeField] private Image    frameImage;  // your card frame

    public void SetupFromData(CardData data)
    {
        if (!data) return;
        if (nameText) nameText.text = data.Name;
        if (descText) descText.text = data.Description;
        if (costText) costText.text = data.Cost.ToString();
        if (artImage)
        {
            artImage.sprite = data.Image;
            artImage.enabled = data.Image != null;
            // Make sure artImage has Preserve Aspect checked in the editor
        }
    }
}