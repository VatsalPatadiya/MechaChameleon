using MechaChameleon.Core;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Network
{
    /// <summary>
    /// Loads scenes in a network-aware way:
    ///   - On server: uses NetworkSceneManager so all clients follow
    ///   - On client: no-op (server drives scene loads)
    /// </summary>
    public static class SceneLoader
    {
        public static void LoadScene(string sceneName)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    sceneName,
                    UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] Only server can load scenes. Requested: {sceneName}");
            }
        }
    }
}
