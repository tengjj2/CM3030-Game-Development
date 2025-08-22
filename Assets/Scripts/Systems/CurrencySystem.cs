using UnityEngine;

public class CurrencySystem : Singleton<CurrencySystem>
{
    [SerializeField] private int startingGold = 100;
    public int Gold { get; private set; }

    private void Start() => Gold = startingGold;

    public bool CanAfford(int price) => Gold >= Mathf.Max(0, price);

    public bool Spend(int price)
    {
        price = Mathf.Max(0, price);
        if (Gold < price) return false;
        Gold -= price;
        // TODO: update gold UI
        return true;
    }

    public void Add(int amount)
    {
        Gold += Mathf.Max(0, amount);
        // TODO: update gold UI
    }
}