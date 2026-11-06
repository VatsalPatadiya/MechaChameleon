// MainMenuUI.cs
// Entry point screen — connects to LobbyUI flow.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MechaChameleon.Core;
using MechaChameleon.Network;

namespace MechaChameleon.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI versionLabel;

        private void Awake()
        {
            if (titleLabel) titleLabel.text = "Paint & Hide";
            if (versionLabel) versionLabel.text = $"v{Application.version}";

            if (playButton) playButton.onClick.AddListener(OnPlay);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettings);
            if (quitButton) quitButton.onClick.AddListener(OnQuit);
        }

        private void OnPlay()
        {
            Network.SceneLoader.LoadScene(SceneConstants.Lobby);
        }

        private void OnSettings()
        {
            // TODO: open settings overlay
            Debug.Log("[MainMenuUI] Settings — not yet implemented");
        }

        private void OnQuit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
