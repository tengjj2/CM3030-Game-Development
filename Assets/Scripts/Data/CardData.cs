using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public int Cost { get; set; }
    [field: SerializeField] public string Description { get; set; }
    [field: SerializeField] public Sprite Image { get; set; }
    [field: SerializeReference, SR] public Effect ManualTargetEffect { get; set; } = null;
    [field: SerializeField] public List<AutoTargetEffect> OtherEffects { get; set; }
    [SerializeField] public bool IsBasic = false; 

    [Header("Lifecycle")]
    [SerializeField] private bool exhaustOnPlay = false;  // <—
    public bool ExhaustOnPlay => exhaustOnPlay;
}
