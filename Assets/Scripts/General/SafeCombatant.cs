using UnityEngine;

public static class SafeCombatant
{
    // Unity "destroyed" objects are not null in C#, so use both checks.
    public static bool IsValid(Object o) => o != null && !o.Equals(null);

    public static bool IsAlive(CombatantView v)
        => IsValid(v) && v.CurrentHealth > 0;

    // Returns true if the performer should abort
    public static bool AbortIfDead(CombatantView v, string where)
    {
        if (!IsAlive(v))
        {
            Debug.Log($"[Guard] Abort at {where}: {(IsValid(v) ? v.name : "null")} not alive/destroyed");
            return true;
        }
        return false;
    }
}