// CameraFollow2D.cs
// Smooth follow for the local player's camera. Attach to the Main Camera.
// Works in 2D top-down portrait view.

using UnityEngine;
using Unity.Netcode;

namespace MechaChameleon.Gameplay.Shared
{
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                TryFindLocalPlayer();
                return;
            }

            Vector3 desired  = _target.position + offset;
            Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
            transform.position = smoothed;
        }

        private void TryFindLocalPlayer()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;
            var local = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (local != null)
                _target = local.transform;
        }
    }
}
