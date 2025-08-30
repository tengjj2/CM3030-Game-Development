using UnityEngine;

public class ForceDiscardGA : GameAction
{
    public int Count;
    public ForceDiscardGA(int count = 1)
    {
        Count = Mathf.Max(1, count);
    }
}