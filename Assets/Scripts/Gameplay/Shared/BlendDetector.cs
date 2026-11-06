using MechaChameleon.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Gameplay
{
    /// <summary>
    /// Attached to each Hunter. Runs on server.
    /// If a Hider is within detectionRadius AND their blend < detectionThreshold,
    /// it notifies the HiderController (and fires a "seeker nearby" warning to the hider).
    /// </summary>
    public class BlendDetector : NetworkBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRadius    = 5f;
        [SerializeField] private float detectionThreshold = 60f; // below this blend %, can be caught

        private float _checkInterval = 0.2f;
        private float _checkTimer;

        private void Update()
        {
            if (!IsServer) return;

            _checkTimer -= Time.deltaTime;
            if (_checkTimer > 0f) return;
            _checkTimer = _checkInterval;

            ScanForHiders();
        }

        [Server]
        private void ScanForHiders()
        {
            var hiders = FindObjectsByType<HiderController>(FindObjectsSortMode.None);
            foreach (var hider in hiders)
            {
                if (hider.Data.Value.IsCaught) continue;

                float dist = Vector2.Distance(transform.position, hider.transform.position);
                bool  near = dist < detectionRadius;
                bool  low  = hider.BlendPercent.Value < detectionThreshold;

                // Warn the hider regardless of blend (they're close)
                NotifyHiderClientRpc(hider.OwnerClientId, near);
            }
        }

        [ClientRpc]
        private void NotifyHiderClientRpc(ulong hiderClientId, bool isNear)
        {
            // Only the hider themselves should react
            if (Unity.Netcode.NetworkManager.Singleton.LocalClientId != hiderClientId) return;

            var hiders = FindObjectsByType<HiderController>(FindObjectsSortMode.None);
            foreach (var h in hiders)
            {
                if (h.OwnerClientId == hiderClientId)
                {
                    // Find the HUD and trigger warning
                    var hud = FindAnyObjectByType<MechaChameleon.UI.HiderHUD>();
                    hud?.ShowSeekerWarning(isNear);
                    break;
                }
            }
        }
    }
}
