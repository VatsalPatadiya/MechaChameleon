using MechaChameleon.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MechaChameleon.Gameplay
{
    /// <summary>
    /// Base networked controller: handles movement via New Input System virtual joystick.
    /// Subclassed by HiderController and HunterController.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float moveSpeed = 4f;

        // Assigned by NetworkGamePlayer on spawn
        public NetworkVariable<PlayerData> Data =
            new NetworkVariable<PlayerData>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        protected Rigidbody2D Rb;
        protected Vector2 MoveInput;

        // ─────────────────────────────────────────────────────────────────────
        public override void OnNetworkSpawn()
        {
            Rb = GetComponent<Rigidbody2D>();

            // Only enable local player's input component
            var inputComp = GetComponent<PlayerInput>();
            if (inputComp != null)
                inputComp.enabled = IsOwner;
        }

        // Called by PlayerInput component (New Input System callback)
        public void OnMove(InputValue value)
        {
            if (!IsOwner) return;
            MoveInput = value.Get<Vector2>();
        }

        protected virtual void FixedUpdate()
        {
            if (!IsOwner) return;
            Rb.linearVelocity = MoveInput * moveSpeed;
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        [ServerRpc]
        protected void UpdateDataServerRpc(PlayerData updatedData)
        {
            Data.Value = updatedData;
        }
    }
}
