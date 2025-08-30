using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card Library")]
public class CardLibrarySO : ScriptableObject
{
    public List<CardData> AllCards = new();

    /// Return the pool used for post-combat rewards (non-basic).
    public List<CardData> GetRewardPool()
        => AllCards.Where(c => c != null && !c.IsBasic).ToList();

    /// Get N distinct random cards from the reward pool.
    public List<CardData> GetRandomRewards(int count)
    {
        var pool = GetRewardPool();
        Shuffle(pool);
        if (count < pool.Count) pool.RemoveRange(count, pool.Count - count);
        return pool;
    }

    // Fisher–Yates
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}