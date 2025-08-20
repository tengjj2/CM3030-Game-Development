using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class EnemyView : CombatantView
{
    [SerializeField] private IntentUI intentUI;         // assign in prefab (the UI above head)
    [SerializeField] private Transform intentAnchor;    // optional: if you need a world anchor
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private GameObject confuseVFXPrefab;

    [Header("Intent Icons")]
    [SerializeField] private Sprite confuseIntentSprite;          // <-- assign a '?' icon in prefab
    [SerializeField] private bool showConfuseStacksAsValue = false;

    public EnemyAI AI { get; private set; }
    public int AttackPower { get; set; }

    /// <summary>
    /// Refreshes the intent UI using the current planned action (with Confuse override).
    /// </summary>
    public void RefreshIntentUI()
    {
        UpdateIntentUI(AI != null ? AI.NextPlanned : null);
    }

    public IEnumerator PlayConfusedAnimation()
    {
        if (this == null || gameObject == null) yield break;

        // Optional VFX spawn
        if (confuseVFXPrefab != null)
        {
            var vfx = Instantiate(confuseVFXPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            Destroy(vfx, 1.0f); // auto-destroy after
        }

        // Shake left/right
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOShakePosition(0.5f, strength: new Vector3(0.2f, 0, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true));

        // Wait for anim to finish
        yield return seq.WaitForCompletion();
    }

    public void UpdateIntentUI(EnemyActionEntry entry)
    {
        if (intentUI == null) return;

        // --- Confuse override: if this enemy has Confuse, show the Confuse icon instead of real intent
        int confuseStacks = GetStatusEffectStacks(StatusEffectType.CONFUSE);
        if (confuseStacks > 0 && confuseIntentSprite != null)
        {
            intentUI.Set(confuseIntentSprite, showConfuseStacksAsValue ? confuseStacks : (int?)null);
            return;
        }

        // --- Normal intent rendering
        if (entry?.action == null)
        {
            intentUI.Hide();
            return;
        }

        var sprite = entry.action.IntentSprite;
        var value = entry.action.GetIntentValue(this);
        intentUI.Set(sprite, value);
    }

    private void Awake()
    {
        AI = GetComponent<EnemyAI>();
    }

    public void InitAI()
    {
        AI?.Init();
        if (AI != null) RefreshIntentUI(); // show intent once after init
    }

    public void PlanNextIntent()
    {
        if (AI == null) return;
        var entry = AI.PlanNextIntent(this);
        UpdateIntentUI(entry); // this method will still override to Confuse if needed
    }

    private bool _setupDone;
    public void Setup(EnemyData enemyData)
    {
        if (_setupDone) { Debug.LogWarning($"[EnemyView] Setup called twice on {name}"); return; }
        _setupDone = true;

        AttackPower = enemyData.AttackPower;
        SetupBase(enemyData.Health, enemyData.Image);

        if (AI == null) AI = GetComponent<EnemyAI>();
        if (AI != null)
        {
            var entries = enemyData.BuildRuntimeMoveset();
            AI.LoadFromData(entries);
            AI.Init();                 // plans once
            RefreshIntentUI();         // show once
        }
        else
        {
            Debug.LogWarning($"[EnemyView] {name} has no EnemyAI component.");
        }
    }
}