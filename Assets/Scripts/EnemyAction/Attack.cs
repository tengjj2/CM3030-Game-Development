using UnityEngine;

[CreateAssetMenu(menuName = "EnemyActions/Attack")]
public class AttackAction : EnemyAction
{
    [Header("Intent")]
    [SerializeField] private Sprite attackIcon;
    public override Sprite IntentSprite => attackIcon;
    public override int? GetIntentValue(EnemyView self) => amount;
    public int amount = 8;
    public override void Enqueue(EnemyView self)
    {
        // Drives the lunge anim → damage GA via your EnemySystem performers
        ActionSystem.Instance.AddReaction(new AttackPlayerGA(self));
        Debug.Log($"[AttackAction] Enqueued AttackPlayerGA for {self.name}");
    }
}