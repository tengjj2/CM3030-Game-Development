using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Speaker")]
public class SpeakerSO : ScriptableObject
{
    [Header("Speaker Info")]
    public string DisplayName;
    public Sprite Portrait;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)] public string[] Greeting; // <-- Add lines directly here
}