using System.Collections;
using UnityEngine;

public class CostSystem : Singleton<CostSystem>
{
    [SerializeField] private CostUI costUI;
    private const int MAX_COST = 3;
    private int currentcost = MAX_COST;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendCostGA>(SpendCostPerformer);
        ActionSystem.AttachPerformer<RefillCostGA>(RefillCostPerformer);
        ActionSystem.SubscribePerformer<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendCostGA>();
        ActionSystem.DetachPerformer<RefillCostGA>();
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public bool HasEnoughCost(int cost)
    {
        return currentcost >= cost;
    }

    private IEnumerator SpendCostPerformer(SpendCostGA spendCostGA)
    {
        currentcost -= spendCostGA.Amount;
        costUI.UpdateCostText(currentcost);
        yield return null;
    }

    private IEnumerator RefillCostPerformer(RefillCostGA refillCostGA)
    {
        currentcost = MAX_COST;
        costUI.UpdateCostText(currentcost);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillCostGA refillCostGA = new();
        ActionSystem.Instance.AddReaction(refillCostGA);
    }
}
