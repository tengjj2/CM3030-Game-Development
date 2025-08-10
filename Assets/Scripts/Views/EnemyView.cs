using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    // [SerializeField] private Image enemyHealthBarFill;

    public int AttackPower { get; set; }

    public void Setup(EnemyData enemyData)
    {
        AttackPower = enemyData.AttackPower;
        UpdateAttackText();
        SetupBase(enemyData.Health, enemyData.Image);

        // // // // Initialize enemy health bar
        // if (enemyHealthBarFill != null)
        // {
        //     float healthPercentage = (float)CurrentHealth / MaxHealth;
        //     enemyHealthBarFill.fillAmount = healthPercentage;
        //     enemyHealthBarFill.color = Color.red; // Different color than player
        // }
    }

//     protected override void UpdateHealthText() 
// {
//     base.UpdateHealthText();
//     if (enemyHealthBarFill != null) 
//     {
//         enemyHealthBarFill.fillAmount = (float)CurrentHealth / MaxHealth;
//     }
// }

    private void UpdateAttackText()
    {
        attackText.text = "ATK: " + AttackPower;
    }
}
