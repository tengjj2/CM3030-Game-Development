using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<CardData> startingDeck;  // if not in PlayerData
    [SerializeField] private RunConfigSO runConfig;        // drag your 10-floor asset here

    private void Start()
    {
        // 1) Player & deck are persistent across the run
        PlayerSystem.Instance.Setup(playerData);
        CardSystem.Instance.Setup(playerData.Deck ?? startingDeck);

        // 2) Hand the run plan to RunManager and start
        RunManager.Instance.StartRun();
    }
}