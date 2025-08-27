using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckChoiceUIController : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform gridParent;
    [SerializeField] private CardChoiceButton buttonPrefab;

    private System.Action onAllChosen;
    private int needCount;
    private List<CardData> chosen = new();

    public void ShowAdd(string title, List<CardData> pool, int count, System.Action<List<CardData>> onDone)
    {
        Clear();
        chosen.Clear();
        needCount = count;
        onAllChosen = () => onDone?.Invoke(new List<CardData>(chosen));

        uiRoot.SetActive(true);
        if (titleText) titleText.text = title;

        foreach (var cd in pool)
        {
            var btn = Instantiate(buttonPrefab, gridParent);
            btn.Setup(cd, () =>
            {
                chosen.Add(cd);
                if (chosen.Count >= needCount) { Hide(); onAllChosen?.Invoke(); }
            });
        }
    }

    public void ShowRemove(string title, List<CardData> deck, int count, System.Action<List<CardData>> onDone)
    {
        Clear();
        chosen.Clear();
        needCount = count;
        onAllChosen = () => onDone?.Invoke(new List<CardData>(chosen));

        uiRoot.SetActive(true);
        if (titleText) titleText.text = title;

        foreach (var cd in deck)
        {
            var btn = Instantiate(buttonPrefab, gridParent);
            btn.Setup(cd, () =>
            {
                chosen.Add(cd);
                if (chosen.Count >= needCount) { Hide(); onAllChosen?.Invoke(); }
            });
        }
    }

    public void Hide()
    {
        uiRoot.SetActive(false);
        Clear();
    }

    private void Clear()
    {
        if (!gridParent) return;
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
    }
}