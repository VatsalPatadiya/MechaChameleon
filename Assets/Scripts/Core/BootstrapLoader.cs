using MechaChameleon.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MechaChameleon.Core
{
    /// <summary>
    /// Entry point. Placed in a minimal Bootstrap scene (scene index 0).
    /// Instantiates persistent singletons, then loads MainMenu.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private NetworkLobbyManager lobbyManagerPrefab;
        [SerializeField] private GameManager         gameManagerPrefab;

        private async void Awake()
        {
            // Spawn persistent managers if not already present
            if (NetworkLobbyManager.Instance == null && lobbyManagerPrefab != null)
                Instantiate(lobbyManagerPrefab);

            if (GameManager.Instance == null && gameManagerPrefab != null)
                Instantiate(gameManagerPrefab);

            // Initialise UGS (sign in anonymously)
            if (NetworkLobbyManager.Instance != null)
                await NetworkLobbyManager.Instance.InitialiseAsync();

            SceneManager.LoadScene(SceneConstants.MainMenu);
        }
    }
}
