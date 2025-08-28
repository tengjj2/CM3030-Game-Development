using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text body;
    [SerializeField] private float typeDelay = 0.02f;

    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    public void HideImmediate()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (body) body.text = string.Empty;
    }

    public IEnumerator ShowLine(string text)
    {
        cg.DOFade(1f, 0.2f);
        cg.interactable = true; cg.blocksRaycasts = true;

        if (body) body.text = "";
        foreach (char c in text)
        {
            if (body) body.text += c;
            yield return new WaitForSeconds(typeDelay);
        }
    }

    public IEnumerator ShowLines(params string[] lines)
    {
        foreach (var line in lines)
            yield return ShowLine(line);
    }

    public void Hide()
    {
        cg.DOFade(0f, 0.2f);
        cg.interactable = false; cg.blocksRaycasts = false;
    }
}