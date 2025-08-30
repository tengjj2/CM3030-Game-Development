public class SpendGoldGA : GameAction
{
    public int Amount { get; private set; }
    public string Reason { get; private set; }
    public bool Success { get; set; }
    public SpendGoldGA(int amount, string reason = null)
    {
        Amount = amount;
        Reason = reason;
        Success = false;
    }
}