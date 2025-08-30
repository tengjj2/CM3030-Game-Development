using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Floor")]
public class FloorSO : ScriptableObject
{
    public FloorType Type;
    [Header("Visuals")]
    public Sprite BackgroundSprite;

    [Header("Combat/Boss")]
    public List<EnemyData> Enemies;

    [Header("Shop")]
    public ShopInventorySO ShopInventory;

    [Header("Lobby")]
    public LobbyOfferSO LobbyOffer;
    public int GoldReward = 50;

    [Header("Meta")]
    [TextArea] public string Title;
    [TextArea] public string Description;
}
