using TMPro;
using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cost;

    public void UpdateCostText(int currentCost)
    {
        cost.text = currentCost.ToString();
    }
}
