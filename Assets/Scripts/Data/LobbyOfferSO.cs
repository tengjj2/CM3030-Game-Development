using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Lobby Offer")]
public class LobbyOfferSO : ScriptableObject
{
    [System.Serializable]
    public class Option
    {
        public string Label;
        public string Description;
        public Sprite Icon;
        public List<LobbyEffectSO> Effects;
    }

    public string Title;
    public string Description;
    public List<Option> Options = new();
}