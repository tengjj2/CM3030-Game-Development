// ModifyCostGA.cs  (generic delta; + for gain, - for loss)
public class ModifyCostGA : GameAction
{
    public int Delta;
    public CombatantView Caster;  
    public CombatantView Target;
    public ModifyCostGA(int delta, CombatantView caster = null, CombatantView target = null)
    {
        Delta = delta;
        Caster = caster;
        Target = target;
    }
}