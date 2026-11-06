using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobby;
using Unity.Services.Lobby.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace MechaChameleon.Network
{
    /// <summary>
    /// Handles UGS sign-in, Relay allocation, Lobby create/join,
    /// and boots the NGO NetworkManager as host or client.
    /// </summary>
    public class NetworkLobbyManager : MonoBehaviour
    {
        public static NetworkLobbyManager Instance { get; private set; }

        [Header("Lobby Config")]
        [SerializeField] private int maxPlayers = 8;
        [SerializeField] private string lobbyName = "PaintAndHide";

        // ── Events ────────────────────────────────────────────────────────────
        public event Action                   OnSignedIn;
        public event Action<string>           OnLobbyCreated;    // lobbyCode
        public event Action<string>           OnLobbyJoined;     // lobbyId
        public event Action<List<Player>>     OnLobbyUpdated;
        public event Action<string>           OnError;

        // ── State ─────────────────────────────────────────────────────────────
        private Lobby     _currentLobby;
        private float     _heartbeatTimer;
        private float     _lobbyPollTimer;
        private const float HeartbeatInterval = 15f;
        private const float PollInterval      = 1.5f;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Initialise UGS ───────────────────────────────────────────────────
        public async Task InitialiseAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                OnSignedIn?.Invoke();
                Debug.Log($"[Lobby] Signed in as {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
        }

        // ── Create Lobby + Relay ─────────────────────────────────────────────
        public async Task CreateLobbyAsync()
        {
            try
            {
                // 1. Allocate Relay
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string joinCode  = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // 2. Create Lobby with relay join code embedded
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                };

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                // 3. Configure NGO transport with Relay
                SetRelayHostData(allocation);
                NetworkManager.Singleton.StartHost();

                OnLobbyCreated?.Invoke(_currentLobby.LobbyCode);
                Debug.Log($"[Lobby] Created lobby {_currentLobby.LobbyCode}");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
        }

        // ── Join Lobby by code ───────────────────────────────────────────────
        public async Task JoinLobbyByCodeAsync(string code)
        {
            try
            {
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
                string relayJoinCode = _currentLobby.Data["RelayJoinCode"].Value;

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                SetRelayClientData(joinAllocation);
                NetworkManager.Singleton.StartClient();

                OnLobbyJoined?.Invoke(_currentLobby.Id);
                Debug.Log($"[Lobby] Joined lobby {_currentLobby.Id}");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
        }

        // ── Heartbeat + Poll ─────────────────────────────────────────────────
        private async void Update()
        {
            if (_currentLobby == null) return;

            // Keep lobby alive (host only)
            if (NetworkManager.Singleton.IsHost)
            {
                _heartbeatTimer += Time.deltaTime;
                if (_heartbeatTimer >= HeartbeatInterval)
                {
                    _heartbeatTimer = 0f;
                    await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
            }

            // Poll for roster changes
            _lobbyPollTimer += Time.deltaTime;
            if (_lobbyPollTimer >= PollInterval)
            {
                _lobbyPollTimer = 0f;
                try
                {
                    _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                    OnLobbyUpdated?.Invoke(_currentLobby.Players);
                }
                catch { /* lobby may have closed */ }
            }
        }

        // ── Relay helpers ────────────────────────────────────────────────────
        private static void SetRelayHostData(Allocation allocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.Url,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);
        }

        private static void SetRelayClientData(JoinAllocation joinAllocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.Url,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);
        }

        public async void LeaveLobby()
        {
            if (_currentLobby == null) return;
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    _currentLobby.Id,
                    AuthenticationService.Instance.PlayerId);
                _currentLobby = null;
            }
            catch { /* ignore */ }
        }
    }
}
