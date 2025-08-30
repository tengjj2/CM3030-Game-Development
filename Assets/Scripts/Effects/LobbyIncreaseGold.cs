using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Effects/Increase Gold")]
public class IncreaseGoldEffect : LobbyEffectSO
{
    public int Amount = 50;

    public override void Apply(System.Action onComplete)
    {
        CurrencySystem.Instance?.Add(Amount, "Lobby boon");
        onComplete?.Invoke();
    }
}