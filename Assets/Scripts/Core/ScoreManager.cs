// ScoreManager.cs
// Server-authoritative score tracking. Attached to GameManager GameObject.

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Core
{
    public class ScoreManager : NetworkBehaviour
    {
        // Points awarded per round
        private const int PointsPerRoundSurvive  = 100;
        private const int PointsPerCatch         = 150;
        private const int PointsHunterNoMisses   = 50; // bonus

        // Persistent across rounds (key = clientId)
        private readonly Dictionary<ulong, int> _cumulativeScores = new();

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        public override void OnNetworkDespawn()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            if (!IsServer) return;
            if (phase == GamePhase.RoundResults)
                TallyRoundScores();
        }

        /// <summary>Called by HunterController when a Hider is caught.</summary>
        public void AddCatchPoints(ulong hunterClientId)
        {
            if (!IsServer) return;
            AddScore(hunterClientId, PointsPerCatch);
        }

        private void TallyRoundScores()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var pd = GetPlayerData(client.ClientId);
                if (pd == null) continue;

                if (pd.Role == PlayerRole.Hider && !pd.IsCaught)
                    AddScore(client.ClientId, PointsPerRoundSurvive);
            }
        }

        private void AddScore(ulong clientId, int points)
        {
            _cumulativeScores.TryGetValue(clientId, out int current);
            _cumulativeScores[clientId] = current + points;
            BroadcastScoreClientRpc(clientId, _cumulativeScores[clientId]);
        }

        public int GetScore(ulong clientId) =>
            _cumulativeScores.TryGetValue(clientId, out int s) ? s : 0;

        [ClientRpc]
        private void BroadcastScoreClientRpc(ulong clientId, int newTotal)
        {
            // UI can listen to this if needed; ScoreManager is server-authoritative
            Debug.Log($"[Score] Client {clientId} total = {newTotal}");
        }

        // Helper: grab PlayerData from NetworkManager connected client
        private PlayerData? GetPlayerData(ulong clientId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(clientId, out var obj)) return null;
            var pc = obj.GetComponent<Gameplay.Shared.PlayerController>();
            if (pc == null) return null;
            return pc.Data.Value;
        }
    }
}
