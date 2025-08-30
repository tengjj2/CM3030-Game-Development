using UnityEngine;
using TMPro;

public class LevelUi : MonoBehaviour
{
    public LevelData levelData; // Global Level
    public TMP_Text levelText;

    void Update()
    {
        if (levelText != null && levelData != null)
        {
            levelText.text = "Level " + levelData.level;
        }
    }
}