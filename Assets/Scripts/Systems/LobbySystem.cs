using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySystem : Singleton<LobbySystem>
{
    [SerializeField] private FloorVisibilityController vis;     // optional
    [SerializeField] private DialoguePanelUI dialoguePanel;     // REQUIRED (Canvas → DialoguePanel)
    [SerializeField] private SpeakerSO receptionist;            // REQUIRED (holds portrait, name, greeting lines)
    [SerializeField] private SpeakerSO playerSpeaker;           // REQUIRED (for player responses)

    /// <summary>Opens the lobby, runs chosen effects, then calls onDone.</summary>
    public void Open(LobbyOfferSO offer, System.Action onDone)
    {
        if (offer == null)
        {
            Debug.LogError("[LobbySystem] Open called with null offer.");
            onDone?.Invoke();
            return;
        }

        vis?.ShowOnlyLobby();
        StartCoroutine(OpenRoutine(offer, onDone));
    }

    private IEnumerator OpenRoutine(LobbyOfferSO offer, System.Action onDone)
    {
        // 1) Receptionist lines (flipped bubble)
        if (receptionist != null && receptionist.Greeting != null && receptionist.Greeting.Length > 0)
        {
            yield return dialoguePanel.ShowNpcSpeechLines(receptionist, receptionist.Greeting, true);
        }
        else
        {
            yield return dialoguePanel.ShowNpcSpeech(receptionist, "Welcome to the lobby! Choose a starting boon.", true);
        }

        // 2) Build choices from LobbyOfferSO
        var choices = new List<LobbyChoice>();
        foreach (var opt in offer.Options)
        {
            if (opt == null) continue;
            choices.Add(new LobbyChoice
            {
                Label       = opt.Label,
                Description = opt.Description,
                Icon        = opt.Icon
            });
        }

        // 3) Flip to player side and show choice buttons
        Debug.Log($"[LobbySystem] ShowPlayerChoices on panel id={dialoguePanel.GetInstanceID()}");
        dialoguePanel.ShowPlayerChoices(playerSpeaker, choices, chosenIndex =>
        {
            Debug.Log($"[LobbySystem] Choice picked index={chosenIndex}"); 
            Debug.Log("[LobbySystem] Choice picked index=" + chosenIndex + " label=" + choices[chosenIndex].Label);
            StartCoroutine(ApplyOptionRoutine(offer, chosenIndex, () =>
            {
                Debug.Log("[LobbySystem] All effects finished, showing Next Floor button");
                dialoguePanel.ClearChoicesUI();
                dialoguePanel.ShowNextFloorButton(
                    onClick: () => {
                        Debug.Log("[LobbySystem] Next Floor clicked");
                        dialoguePanel.Hide();
                        vis?.ShowOnlyCombat();
                        onDone?.Invoke();
                    },
                    label: "Next Floor"
                );
            }));
        });
    }

    private IEnumerator ApplyOptionRoutine(LobbyOfferSO offer, int index, System.Action onDone)
    {
        if (offer == null || offer.Options == null || index < 0 || index >= offer.Options.Count)
        {
            Debug.LogWarning("[LobbySystem] Invalid option index or no options.");
            onDone?.Invoke();
            yield break;
        }

        var chosen = offer.Options[index];
        Debug.Log($"[LobbySystem] Applying choice: {chosen.Label} (effects={chosen.Effects?.Count ?? 0})");

        if (chosen.Effects != null)
        {
            for (int i = 0; i < chosen.Effects.Count; i++)
            {
                var eff = chosen.Effects[i];
                if (eff == null) { Debug.LogWarning($"[LobbySystem] Effect {i} is null, skipping."); continue; }

                bool finished = false;
                Debug.Log($"[LobbySystem] -> Start effect {i}: {eff.name}");
                eff.Apply(() => { Debug.Log($"[LobbySystem] <- Effect {i} callback: {eff.name}"); finished = true; });

                float t = 0f, timeout = 15f;
                while (!finished && t < timeout) { t += Time.unscaledDeltaTime; yield return null; }

                if (!finished)
                    Debug.LogWarning($"[LobbySystem] Effect {eff.name} timed out (no onComplete?). Forcing continue.");
            }
        }

        onDone?.Invoke();
    }

}
