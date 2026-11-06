using System.Collections.Generic;
using System.Linq;
using MechaChameleon.Core;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Network
{
    /// <summary>Server: 1 Hunter per 4 players, rest Hiders. Runs on Countdown phase.</summary>
    public class RoleAssigner : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            if (GameManager.Instance != null)
                GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GameManager.GamePhase phase)
        {
            if (phase == GameManager.GamePhase.Countdown) AssignRoles();
        }

        [Server]
        private void AssignRoles()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            var ids = clients.Select(c => c.ClientId).ToList();
            Shuffle(ids);
            int hunterCount = Mathf.Max(1, ids.Count / 4);
            var hunters = ids.Take(hunterCount).ToHashSet();

            foreach (var client in clients)
            {
                var ctrl = client.PlayerObject?.GetComponent<PlayerController>();
                if (ctrl == null) continue;
                var d = ctrl.Data.Value;
                d.Role = hunters.Contains(client.ClientId) ? PlayerRole.Hunter : PlayerRole.Hider;
                ctrl.Data.Value = d;
            }
            Debug.Log($"[Roles] {hunterCount} hunter(s), {ids.Count - hunterCount} hider(s)");
        }

        /// <summary>Returns the assigned role for a client. Called by SpawnManager.</summary>
        public PlayerRole GetRole(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return PlayerRole.Hider;
            var client = NetworkManager.Singleton.ConnectedClientsList
                .FirstOrDefault(c => c.ClientId == clientId);
            if (client == null) return PlayerRole.Hider;
            var ctrl = client.PlayerObject?.GetComponent<PlayerController>();
            return ctrl != null ? ctrl.Data.Value.Role : PlayerRole.Hider;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
