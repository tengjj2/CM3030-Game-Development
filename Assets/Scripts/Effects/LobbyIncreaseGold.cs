using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Increase Gold")]
public class IncreaseGoldEffect : LobbyEffectSO
{
    public int Amount = 50;

    void Apply()
    {
        CurrencySystem.Instance?.Add(Amount);
    }
}