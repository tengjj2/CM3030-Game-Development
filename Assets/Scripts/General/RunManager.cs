using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RunManager : Singleton<RunManager>
{
    // NEW: floor lifecycle events
    [SerializeField] private FloorVisibilityController visibility;
    [SerializeField] private BackgroundController background;
    [SerializeField] private RunConfigSO runConfig;   // <- use this instead of FloorSO[]
    [SerializeField] private PlayerData playerData;

    private int index = 0;

    public FloorSO CurrentFloor { get; private set; }

    public void StartRun()
    {
        PlayerSystem.Instance.InitializeRun(playerData);
        index = 0;
        NextFloor();
    }

    public void NextFloor()
    {
        if (runConfig == null || runConfig.Floors == null || index >= runConfig.Floors.Count)
        {
            Debug.Log("Run complete!");
            return;
        }
        TurnSystem.Instance.SuspendCombat();                 // put TurnSystem into Inactive
        DiscardPromptUI.Instance?.Hide();                    // just in case the prompt is up
        CardSystem.Instance.RebuildForNewCombatFromRunDeck(
            CardSystem.Instance.RunDeckDataRO,
            shuffle: true
        );        
        CostSystem.Instance.Refill(); 
        LoadFloor(runConfig.Floors[index++]);
    }

    public void LoadFloor(FloorSO floor)
    {
        CurrentFloor = floor; 
        if (floor.Type == FloorType.Lobby)
        {
            visibility?.ShowOnlyLobby();
            BackgroundController.Instance?.SetBackground(floor.BackgroundSprite);
            LobbySystem.Instance.Open(floor.LobbyOffer, () =>
            {
                NextFloor();
            });
            return;
        }

        if (floor.Type == FloorType.Combat|| floor.Type == FloorType.Boss)
        {
            visibility?.ShowOnlyCombat();
            BackgroundController.Instance?.SetBackground(floor.BackgroundSprite);

            // NEW: rebuild runtime piles from the persisted run deck
            CardSystem.Instance?.RebuildForNewCombatFromRunDeck(CardSystem.Instance.RunDeckDataRO, shuffle: true);

            EnemySystem.Instance.Setup(floor.Enemies);

            // Play music based on type
            if (floor.Type == FloorType.Boss)
                AudioManager.Instance?.PlayBGM("bgm_boss");

            // Start combat; use your preferred entry point
            TurnSystem.Instance.BeginCombat();   // or BeginMatch() if that’s your wrapper
            return;
        }

        Debug.LogWarning($"[RunManager] Unhandled floor type: {floor.Type}");
    }

}