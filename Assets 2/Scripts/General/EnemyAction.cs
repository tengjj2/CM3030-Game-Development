using UnityEngine;

public abstract class EnemyAction : ScriptableObject
{
    [TextArea] public string TelegraphText; // optional: intent UI
    // NEW: override to provide the icon shown above the enemy
    public virtual Sprite IntentSprite => null;

    // NEW: override to provide the number displayed (damage, block, stacks, etc.)
    // return null if you don’t want a number.
    public virtual int? GetIntentValue(EnemyView self) => null;

    public abstract void Enqueue(EnemyView self);
}