using UnityEngine;

public abstract class LobbyEffectSO : ScriptableObject
{
    public virtual void Apply(System.Action onComplete)
    {
        onComplete?.Invoke();
    }
}