using UnityEngine;

public class BackgroundManager : Singleton<BackgroundManager>
{
    [SerializeField] private SpriteRenderer backgroundRenderer;

    protected override void Awake()
    {
        base.Awake();
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetBackground(Sprite sprite)
    {
        if (backgroundRenderer == null)
        {
            Debug.LogWarning("[BackgroundManager] No backgroundRenderer assigned.");
            return;
        }

        if (sprite != null)
        {
            backgroundRenderer.sprite = sprite;
            backgroundRenderer.enabled = true;
        }
        else
        {
            backgroundRenderer.enabled = false;
        }
    }
}