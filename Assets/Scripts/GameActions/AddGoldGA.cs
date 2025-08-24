public class AddGoldGA : GameAction
{
    public int Amount { get; private set; }
    public string Reason { get; private set; }
    public AddGoldGA(int amount, string reason = null)
    {
        Amount = amount;
        Reason = reason;
    }
}