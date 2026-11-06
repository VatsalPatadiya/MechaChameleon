using System.Collections.Generic;
using MechaChameleon.Network;
using TMPro;
using Unity.Services.Lobby.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MechaChameleon.UI
{
    /// <summary>
    /// Lobby screen: create or join a room, show player list, ready up.
    /// Talks to NetworkLobbyManager.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private GameObject lobbyPanel;

        [Header("Join Panel")]
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button         createButton;
        [SerializeField] private Button         joinButton;
        [SerializeField] private TMP_Text       statusLabel;

        [Header("Lobby Panel")]
        [SerializeField] private TMP_Text  lobbyCodeLabel;
        [SerializeField] private Transform playerListRoot;
        [SerializeField] private GameObject playerRowPrefab;
        [SerializeField] private Button    readyButton;
        [SerializeField] private Button    startButton; // host only

        // ─────────────────────────────────────────────────────────────────────
        private async void Start()
        {
            ShowJoinPanel();

            createButton.onClick.AddListener(OnCreateClicked);
            joinButton.onClick.AddListener(OnJoinClicked);

            var mgr = NetworkLobbyManager.Instance;
            if (mgr == null)
            {
                SetStatus("NetworkLobbyManager not found in scene.");
                return;
            }

            mgr.OnSignedIn      += ()    => SetStatus("Signed in. Ready.");
            mgr.OnLobbyCreated  += code  => { SetStatus($"Room code: {code}"); ShowLobbyPanel(code); };
            mgr.OnLobbyJoined   += id    => { SetStatus("Joined!"); ShowLobbyPanel("----"); };
            mgr.OnLobbyUpdated  += list  => RefreshPlayerList(list);
            mgr.OnError         += msg   => SetStatus($"Error: {msg}");

            await mgr.InitialiseAsync();
        }

        // ── Panels ────────────────────────────────────────────────────────────
        private void ShowJoinPanel()
        {
            joinPanel.SetActive(true);
            lobbyPanel.SetActive(false);
        }

        private void ShowLobbyPanel(string code)
        {
            joinPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            lobbyCodeLabel.text = $"Code: {code}";
        }

        // ── Buttons ───────────────────────────────────────────────────────────
        private async void OnCreateClicked()
        {
            SetStatus("Creating room…");
            createButton.interactable = false;
            joinButton.interactable   = false;
            await NetworkLobbyManager.Instance.CreateLobbyAsync();
            createButton.interactable = true;
            joinButton.interactable   = true;
        }

        private async void OnJoinClicked()
        {
            var code = codeInput.text.Trim().ToUpper();
            if (code.Length < 4) { SetStatus("Enter a valid code."); return; }

            SetStatus("Joining…");
            createButton.interactable = false;
            joinButton.interactable   = false;
            await NetworkLobbyManager.Instance.JoinLobbyByCodeAsync(code);
            createButton.interactable = true;
            joinButton.interactable   = true;
        }

        // ── Player list ───────────────────────────────────────────────────────
        private void RefreshPlayerList(List<Player> players)
        {
            foreach (Transform child in playerListRoot)
                Destroy(child.gameObject);

            foreach (var p in players)
            {
                var row  = Instantiate(playerRowPrefab, playerListRoot);
                var text = row.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = p.Id[..8]; // show first 8 chars of player ID
            }
        }

        private void SetStatus(string msg)
        {
            if (statusLabel != null) statusLabel.text = msg;
            Debug.Log($"[LobbyUI] {msg}");
        }
    }
}
