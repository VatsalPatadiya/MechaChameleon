// NetworkManagerConfig.cs
// Attach this to the same GameObject as NetworkManager in the Bootstrap scene.
// It wires up Unity Transport with Relay support and sets heartbeat timing.
// Called automatically by BootstrapLoader BEFORE any UGS calls.

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MechaChameleon.Network
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkManagerConfig : MonoBehaviour
    {
        [Header("Transport")]
        [SerializeField] private UnityTransport transport;

        [Header("Timing")]
        [SerializeField] private int tickRate = 30; // server ticks per second
        [SerializeField] private float clientTimeout = 10f;

        private NetworkManager _nm;

        private void Awake()
        {
            _nm = GetComponent<NetworkManager>();

            if (transport == null)
                transport = GetComponent<UnityTransport>();

            ApplyConfig();
        }

        private void ApplyConfig()
        {
            if (_nm == null || transport == null) return;

            // UTP default port — Relay overrides this at runtime
            transport.SetConnectionData("127.0.0.1", 7777);

            _nm.NetworkConfig.TickRate = (uint)tickRate;
            _nm.NetworkConfig.ClientConnectionBufferTimeout = clientTimeout;

            // Scenes managed by NGO's NetworkSceneManager
            _nm.NetworkConfig.EnableSceneManagement = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_nm == null) _nm = GetComponent<NetworkManager>();
            if (transport == null) transport = GetComponent<UnityTransport>();
        }
#endif
    }
}
