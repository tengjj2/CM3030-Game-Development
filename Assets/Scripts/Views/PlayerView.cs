using UnityEngine;

public class PlayerView : CombatantView
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public void Setup(PlayerData playerData)
    {
                // Set sprite first if needed
        if (spriteRenderer != null && playerData.Image != null)
        {
            spriteRenderer.sprite = playerData.Image;
        }

        SetupBase(playerData.Health);
    }
}
