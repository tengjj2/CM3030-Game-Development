using UnityEngine;

public class PlayerView : CombatantView
{
    public void Setup(PlayerData playerData)
    {
        SetupBase(playerData.Health, playerData.Image);
    }
}
