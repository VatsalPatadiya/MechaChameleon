// CountdownUI.cs
// Overlay shown during the Countdown phase.
// Subscribes to GameManager.CurrentPhase and PhaseTimer NetworkVariables.

using TMPro;
using UnityEngine;
using Unity.Netcode;
using MechaChameleon.Core;

namespace MechaChameleon.UI
{
    public class CountdownUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI countdownLabel;
        [SerializeField] private TextMeshProUGUI roleLabel;

        private void Start()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CurrentPhase.OnValueChanged  += OnPhaseChanged;
            GameManager.Instance.PhaseTimer.OnValueChanged    += OnTimerChanged;

            // Initialise in case we spawned mid-phase
            OnPhaseChanged(GamePhase.Lobby, GameManager.Instance.CurrentPhase.Value);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CurrentPhase.OnValueChanged  -= OnPhaseChanged;
            GameManager.Instance.PhaseTimer.OnValueChanged    -= OnTimerChanged;
        }

        private void OnPhaseChanged(GamePhase prev, GamePhase next)
        {
            bool show = next == GamePhase.Countdown;
            panel.SetActive(show);

            if (show && NetworkManager.Singleton != null)
            {
                ulong localId = NetworkManager.Singleton.LocalClientId;
                // Try to read role from PlayerController on local player object
                if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()
                    is { } playerObj)
                {
                    var pc = playerObj.GetComponent<Gameplay.Shared.PlayerController>();
                    if (pc != null)
                    {
                        string roleName = pc.Data.Value.Role switch
                        {
                            PlayerRole.Hunter => "🔫 You are the HUNTER",
                            PlayerRole.Hider  => "🎨 You are a HIDER",
                            _                 => "Waiting for role…"
                        };
                        if (roleLabel) roleLabel.text = roleName;
                    }
                }
            }
        }

        private void OnTimerChanged(float prev, float next)
        {
            if (!panel.activeSelf) return;
            if (countdownLabel) countdownLabel.text = Mathf.CeilToInt(next).ToString();
        }
    }
}
