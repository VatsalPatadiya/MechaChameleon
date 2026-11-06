// SpawnManager.cs
// Server-side: spawns player prefabs at designated spawn points when the
// Countdown phase begins, or when a client connects during Lobby.

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using MechaChameleon.Core;

namespace MechaChameleon.Gameplay.Shared
{
    public class SpawnManager : NetworkBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject hiderPrefab;
        [SerializeField] private GameObject hunterPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] hiderSpawnPoints;
        [SerializeField] private Transform[] hunterSpawnPoints;

        private readonly Dictionary<ulong, GameObject> _spawnedPlayers = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            NetworkManager.Singleton.OnClientConnectedCallback    += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback   += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback    -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback   -= OnClientDisconnected;
            }
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (!IsServer) return;
            if (phase == GamePhase.Countdown)
                SpawnAllPlayers();
        }

        private void OnClientConnected(ulong clientId)
        {
            // During Lobby, players join but are not spawned yet.
            // Spawning happens on Countdown.
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            if (_spawnedPlayers.TryGetValue(clientId, out var obj))
            {
                if (obj != null)
                    obj.GetComponent<NetworkObject>()?.Despawn(true);
                _spawnedPlayers.Remove(clientId);
            }
        }

        private void SpawnAllPlayers()
        {
            if (!IsServer) return;

            int hiderIndex  = 0;
            int hunterIndex = 0;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong id = client.ClientId;
                if (_spawnedPlayers.ContainsKey(id)) continue; // already spawned

                // Role is set by RoleAssigner before Countdown fires SpawnAllPlayers.
                // We read it from connected client's existing lobby data.
                // Default to Hider if role not yet assigned (edge case).
                PlayerRole role = GetRole(id);

                GameObject prefab  = role == PlayerRole.Hunter ? hunterPrefab : hiderPrefab;
                Transform[] points = role == PlayerRole.Hunter ? hunterSpawnPoints : hiderSpawnPoints;

                int idx = role == PlayerRole.Hunter ? hunterIndex++ : hiderIndex++;
                Vector3 pos = (points != null && points.Length > 0)
                    ? points[idx % points.Length].position
                    : Vector3.zero;

                var go = Instantiate(prefab, pos, Quaternion.identity);
                var no = go.GetComponent<NetworkObject>();
                no.SpawnAsPlayerObject(id, true);
                _spawnedPlayers[id] = go;
            }
        }

        private PlayerRole GetRole(ulong clientId)
        {
            // RoleAssigner stores per-client roles; ask it directly.
            var ra = FindObjectOfType<RoleAssigner>();
            return ra != null ? ra.GetRole(clientId) : PlayerRole.Hider;
        }
    }
}
