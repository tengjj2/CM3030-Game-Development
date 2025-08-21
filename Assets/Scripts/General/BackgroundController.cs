using UnityEngine;

public class BackgroundController : Singleton<BackgroundController>
{
    [SerializeField] private SpriteRenderer backgroundRenderer; // assign your BG SpriteRenderer

    public void SetBackground(Sprite sprite)
    {
        if (!backgroundRenderer) return;
        backgroundRenderer.sprite = sprite;
    }
}