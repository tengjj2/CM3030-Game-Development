using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class EnemyView : CombatantView
{
    [Header("Enemy Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Enemy UI")]
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private Canvas worldCanvas; // For world-space UI
    
    public int AttackPower { get; private set; }
    public EnemyData EnemyData { get; private set; }

    private void Awake()
    {
        // Auto-get components if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (worldCanvas == null)
            worldCanvas = GetComponentInChildren<Canvas>();
        
        // Ensure world canvas is set up correctly
        if (worldCanvas != null)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingLayerName = "UI"; // Adjust as needed
        }
    }

    public void Setup(EnemyData enemyData)
    {
        EnemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        
        // Set sprite
        if (spriteRenderer != null && enemyData.Image != null)
        {
            spriteRenderer.sprite = enemyData.Image;
        }
        
        // Initialize base health
        SetupBase(enemyData.Health);
        
        // Update attack text
        UpdateAttackText();
        
        // Start idle animation if using animator
        // if (useAnimator && animator != null)
        // {
        //     animator.SetTrigger("idle");
        // }
        animator.Play("idle"); 
    }

    private void UpdateAttackText()
    {
        if (attackText != null)
        {
            attackText.text = $"ATK: {AttackPower}";
        }
    }

    public void PlayAttackAnimation()
    {
        animator.Play("attack");
    }

}