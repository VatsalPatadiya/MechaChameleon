// ResultsUI.cs
// Shown at RoundResults and FinalResults phases.
// Bind() is called by a scene controller; populates the scoreboard rows.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MechaChameleon.Core;
using MechaChameleon.Network;

namespace MechaChameleon.UI
{
    public class ResultsUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject roundResultsPanel;
        [SerializeField] private GameObject finalResultsPanel;

        [Header("Round Results")]
        [SerializeField] private Transform scoreRowContainer;
        [SerializeField] private GameObject scoreRowPrefab; // TMP label + score label
        [SerializeField] private TextMeshProUGUI roundHeaderLabel;

        [Header("Final Results")]
        [SerializeField] private TextMeshProUGUI winnerLabel;
        [SerializeField] private Transform finalRowContainer;
        [SerializeField] private Button mainMenuButton;

        private ScoreManager _scoreManager;

        private void Awake()
        {
            if (mainMenuButton)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void ShowRoundResults(int roundIndex, List<(string name, int score, bool survived)> rows)
        {
            roundResultsPanel.SetActive(true);
            finalResultsPanel.SetActive(false);

            roundHeaderLabel.text = $"Round {roundIndex} Results";

            ClearRows(scoreRowContainer);
            foreach (var row in rows)
                AddRow(scoreRowContainer, row.name, row.score, row.survived ? ColourConstants.MintHider : Color.white);
        }

        public void ShowFinalResults(List<(string name, int score)> rows)
        {
            roundResultsPanel.SetActive(false);
            finalResultsPanel.SetActive(true);

            ClearRows(finalRowContainer);
            int rank = 1;
            foreach (var row in rows)
            {
                AddRow(finalRowContainer, $"{rank}. {row.name}", row.score, rank == 1 ? ColourConstants.GoldScore : Color.white);
                rank++;
            }

            if (rows.Count > 0)
                winnerLabel.text = $"🏆 {rows[0].name} Wins!";
        }

        private void ClearRows(Transform container)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void AddRow(Transform container, string playerName, int score, Color nameColour)
        {
            if (scoreRowPrefab == null) return;
            var row = Instantiate(scoreRowPrefab, container);
            var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length >= 2)
            {
                labels[0].text = playerName;
                labels[0].color = nameColour;
                labels[1].text = score.ToString();
            }
        }

        private void OnMainMenuClicked()
        {
            Network.SceneLoader.LoadScene(SceneConstants.MainMenu);
        }
    }
}
