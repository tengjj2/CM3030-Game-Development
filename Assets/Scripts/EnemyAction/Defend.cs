using UnityEngine;

[CreateAssetMenu(menuName = "EnemyActions/Block")]
public class BlockAction : EnemyAction
{
    [Header("Intent")]
    [SerializeField] private Sprite blockIcon;
    public override Sprite IntentSprite => blockIcon;
    public override int? GetIntentValue(EnemyView self) => amount;
    public int amount = 8;
    public BuffAction.TargetingMode targetMode = BuffAction.TargetingMode.Self;
    public int specificEnemyIndex = 0;

    public override void Enqueue(EnemyView self)
    {
        self.GainBlock(amount, self);
    }
}