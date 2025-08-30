using UnityEngine;
public enum TickPhase { StartOfTurn, EndOfTurn }

public class TickStatusesGA:GameAction 
{
    public CombatantView Target;
    public TickPhase Phase;
    public bool IsOwnersTurn;

    public TickStatusesGA(CombatantView target, TickPhase phase, bool isOwnersTurn)
    {
        Target = target;
        Phase = phase;
        IsOwnersTurn = isOwnersTurn;
    }
}