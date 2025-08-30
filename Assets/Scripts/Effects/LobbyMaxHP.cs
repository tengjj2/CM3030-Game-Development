// Instant effect (always call onComplete)
using UnityEngine;
public class IncreaseMaxHpEffect : LobbyEffectSO
{
    public int Amount = 20;
    public override void Apply(System.Action onComplete)
    {
        try
        {
            var pv = PlayerSystem.Instance?.PlayerView;
            if (pv != null)
            {
               pv.IncreaseMaxHealth(Amount, healBySameAmount: true);
            }
        }
        finally { onComplete?.Invoke(); }
        Debug.Log($"Finishing Max hp effect");
    }
}