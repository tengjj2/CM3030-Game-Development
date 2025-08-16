using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private IntentUI intentUI;         // assign in prefab (the UI above head)
    [SerializeField] private Transform intentAnchor;    // optional: if you need a world anchor
    [SerializeField] private TMP_Text attackText;

    public EnemyAI AI { get; private set; }
    public int AttackPower { get; set; }

    public void UpdateIntentUI(EnemyActionEntry entry)
    {
        if (intentUI == null) return;

        if (entry?.action == null)
        {
            intentUI.Hide();
            return;
        }

        var sprite = entry.action.IntentSprite;
        var value  = entry.action.GetIntentValue(this);
        intentUI.Set(sprite, value);
    }

    private void Awake()
    {
        AI = GetComponent<EnemyAI>();
    }

    public void InitAI()
    {
        AI?.Init();
        // NEW: refresh telegraph after init (in case someone calls InitAI directly)
        if (AI != null) UpdateIntentUI(AI.NextPlanned);
    }

    public void PlanNextIntent()
    {
        if (AI == null) return;
        var entry = AI.PlanNextIntent(this);
        UpdateIntentUI(entry);
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
            AI.Init(); // plans once
            UpdateIntentUI(AI.NextPlanned); // show once
        }
    }
}