using MechaChameleon.Core;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Gameplay
{
    /// <summary>
    /// Hider-specific logic:
    ///   - Tracks blend percentage (0–100) based on colour match
    ///   - Exposes Paint action (opens colour picker, deducts charge)
    ///   - Whistle charge management
    ///   - Caught detection (server authoritative)
    /// </summary>
    public class HiderController : PlayerController
    {
        // ── Network state ─────────────────────────────────────────────────────
        public NetworkVariable<float> BlendPercent =
            new NetworkVariable<float>(0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        public NetworkVariable<int> WhistleCharges =
            new NetworkVariable<int>(1,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        // ── Config ────────────────────────────────────────────────────────────
        [Header("Hider Settings")]
        [SerializeField] private float maxBlend = 100f;
        [SerializeField] private float blendDecayRate = 2f;    // per second while moving
        [SerializeField] private float blendBuildRate = 8f;    // per second while still + painted
        [SerializeField] private int   maxWhistleCharges = 1;

        // Current paint colour chosen by this hider (synced)
        public NetworkVariable<Color> PaintColour =
            new NetworkVariable<Color>(Color.white,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        // Local state
        private bool _isMoving;
        private bool _isPainting;

        // ─────────────────────────────────────────────────────────────────────
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
                WhistleCharges.Value = maxWhistleCharges;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!IsServer) return;
            _isMoving = MoveInput.sqrMagnitude > 0.01f;
            UpdateBlend();
        }

        [Server]
        private void UpdateBlend()
        {
            if (_isMoving)
            {
                BlendPercent.Value = Mathf.Max(0f, BlendPercent.Value - blendDecayRate * Time.fixedDeltaTime);
            }
            else if (_isPainting)
            {
                BlendPercent.Value = Mathf.Min(maxBlend, BlendPercent.Value + blendBuildRate * Time.fixedDeltaTime);
            }
        }

        // ── Actions (called locally, ServerRpc to validate) ──────────────────

        /// <summary>Player taps Paint button — apply colour to blend.</summary>
        [ServerRpc(RequireOwnership = true)]
        public void StartPaintingServerRpc()
        {
            _isPainting = true;
        }

        [ServerRpc(RequireOwnership = true)]
        public void StopPaintingServerRpc()
        {
            _isPainting = false;
        }

        /// <summary>Use whistle charge — sends audio ping to all clients.</summary>
        [ServerRpc(RequireOwnership = true)]
        public void UseWhistleServerRpc()
        {
            if (WhistleCharges.Value <= 0) return;
            WhistleCharges.Value--;
            PlayWhistleClientRpc();
        }

        [ClientRpc]
        private void PlayWhistleClientRpc()
        {
            // TODO: play whistle SFX and show visual ping on minimap
            Debug.Log($"[Hider] Whistle used by {OwnerClientId}");
        }

        /// <summary>Server marks this hider as caught.</summary>
        [Server]
        public void MarkCaught()
        {
            var d = Data.Value;
            d.IsCaught = true;
            Data.Value = d;
            OnCaughtClientRpc();
        }

        [ClientRpc]
        private void OnCaughtClientRpc()
        {
            // TODO: trigger caught VFX / UI
            Debug.Log($"[Hider] Caught! ClientId={OwnerClientId}");
        }
    }
}
