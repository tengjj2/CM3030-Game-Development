// using DG.Tweening;
// using TMPro;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.UI;

// public class CombatantView : MonoBehaviour
// {
//     [SerializeField] private TMP_Text healthText;
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private Image healthBarFill;
//     public int MaxHealth { get; private set; }
//     public int CurrentHealth { get; private set; }

//     protected void SetupBase(int health, Sprite image)
//     {
//         MaxHealth = CurrentHealth = health;
//         spriteRenderer.sprite = image;
//         UpdateHealthText();
//     }

//     private void UpdateHealthText()
//     {
//         healthText.text = "HP: " + CurrentHealth;

//         // Update health bar
//         if (healthBarFill != null)
//         {
//             float healthPercentage = (float)CurrentHealth / MaxHealth;
//             healthBarFill.fillAmount = healthPercentage;
//         }
//     }

//     public void Damage(int damageAmount)
//     {
//         CurrentHealth -= damageAmount;
//         if (CurrentHealth < 0)
//         {
//             CurrentHealth = 0;
//         }
//         transform.DOShakePosition(0.2f, 0.5f);

//                 // Update health bar
//         if (healthBarFill != null)
//         {
//             float healthPercentage = (float)CurrentHealth / MaxHealth;
//             healthBarFill.fillAmount = healthPercentage;
//         }
//         UpdateHealthText();
//     }
// }
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class CombatantView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected TMP_Text healthText;
    [SerializeField] protected Image healthBarFill;
    
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    protected virtual void SetupBase(int health)
    {
        MaxHealth = CurrentHealth = health;
        UpdateHealthDisplay();
    }

    protected void UpdateHealthDisplay()
    {
        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{CurrentHealth}/{MaxHealth}";
        }

        // Update health bar
        if (healthBarFill != null)
        {
            float healthPercentage = (float)CurrentHealth / MaxHealth;
            healthBarFill.fillAmount = healthPercentage;
        }
    }

    public virtual void Damage(int damageAmount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        
        // Visual feedback
        transform.DOShakePosition(0.2f, 0.5f, 10, 90, false, true);
        
        UpdateHealthDisplay();
    }
    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }
}