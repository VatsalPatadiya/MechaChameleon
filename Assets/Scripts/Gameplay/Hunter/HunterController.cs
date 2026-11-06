using MechaChameleon.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MechaChameleon.Gameplay
{
    /// <summary>
    /// Hunter-specific logic:
    ///   - Twin-stick: left stick = move (PlayerController base), right stick = aim
    ///   - SHOOT fires a paint splat at the aimed target
    ///   - Proximity warmth detection
    ///   - Lure whistle usage
    /// </summary>
    public class HunterController : PlayerController
    {
        // ── Config ────────────────────────────────────────────────────────────
        [Header("Hunter Settings")]
        [SerializeField] private float shootRange = 3f;
        [SerializeField] private float aimSensitivity = 1f;
        [SerializeField] private float shootCooldown = 0.8f;
        [SerializeField] private LayerMask hiderLayerMask;

        // ── Network state ─────────────────────────────────────────────────────
        public NetworkVariable<Vector2> AimDirection =
            new NetworkVariable<Vector2>(Vector2.up,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        public NetworkVariable<float> HeatLevel =
            new NetworkVariable<float>(0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        // ── Local state ───────────────────────────────────────────────────────
        private Vector2 _aimInput;
        private float   _shootTimer;

        // ─────────────────────────────────────────────────────────────────────
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        // Called by PlayerInput (New Input System)
        public void OnAim(InputValue value)
        {
            if (!IsOwner) return;
            _aimInput = value.Get<Vector2>();
            if (_aimInput.sqrMagnitude > 0.01f)
                AimDirection.Value = _aimInput.normalized;
        }

        public void OnShoot(InputValue value)
        {
            if (!IsOwner) return;
            if (value.isPressed)
                TryShootServerRpc();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsServer) return;
            _shootTimer -= Time.fixedDeltaTime;
            UpdateHeatLevel();
        }

        // ── Shooting ─────────────────────────────────────────────────────────
        [ServerRpc(RequireOwnership = true)]
        private void TryShootServerRpc()
        {
            if (_shootTimer > 0f) return;

            _shootTimer = shootCooldown;

            // Raycast in aim direction for a hider
            var hit = Physics2D.Raycast(
                transform.position,
                AimDirection.Value,
                shootRange,
                hiderLayerMask);

            if (hit.collider != null)
            {
                var hider = hit.collider.GetComponent<HiderController>();
                if (hider != null && !hider.Data.Value.IsCaught)
                {
                    // Only catch if hider's blend is low enough
                    if (hider.BlendPercent.Value < 85f)
                    {
                        hider.MarkCaught();
                        AwardTagScoreClientRpc();
                    }
                }
            }

            SpawnSplatClientRpc(transform.position, AimDirection.Value);
        }

        [ClientRpc]
        private void SpawnSplatClientRpc(Vector2 origin, Vector2 direction)
        {
            // TODO: spawn paint splat VFX along trajectory
            Debug.Log($"[Hunter] Splat fired from {origin} dir {direction}");
        }

        [ClientRpc]
        private void AwardTagScoreClientRpc()
        {
            // TODO: show score popup on hunter's HUD
        }

        // ── Heat level (proximity to nearest hider) ───────────────────────────
        [Server]
        private void UpdateHeatLevel()
        {
            var hiders = FindObjectsByType<HiderController>(FindObjectsSortMode.None);
            float closestDist = float.MaxValue;

            foreach (var h in hiders)
            {
                if (h.Data.Value.IsCaught) continue;
                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < closestDist) closestDist = d;
            }

            // Normalise: within 1 unit = 1.0, beyond 10 units = 0.0
            HeatLevel.Value = Mathf.Clamp01(1f - (closestDist - 1f) / 9f);
        }
    }
}
