using UnityEngine;

public class testlevel : MonoBehaviour
{

    public LevelData levelData; // Global Level

    public void OnClick()
    {
        levelData.level += 1;
        Debug.Log("Level increased to " + levelData.level);
    }
}