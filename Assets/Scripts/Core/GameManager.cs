using System;
using Unity.Netcode;
using UnityEngine;

namespace MechaChameleon.Core
{
    /// <summary>
    /// Singleton that owns high-level game state (lobby → round → results).
    /// Runs on the server; clients read NetworkVariables.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Game phases ──────────────────────────────────────────────────────
        public enum GamePhase
        {
            Lobby,
            Countdown,
            HidePhase,
            HuntPhase,
            RoundResults,
            FinalResults
        }

        public NetworkVariable<GamePhase> CurrentPhase =
            new NetworkVariable<GamePhase>(
                GamePhase.Lobby,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        public NetworkVariable<float> PhaseTimer =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        // ── Config ───────────────────────────────────────────────────────────
        [Header("Round Settings")]
        [SerializeField] private float countdownDuration = 5f;
        [SerializeField] private float hidePhaseDuration = 60f;
        [SerializeField] private float huntPhaseDuration = 120f;
        [SerializeField] private float resultsDuration = 10f;

        public event Action<GamePhase> OnPhaseChanged;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            CurrentPhase.OnValueChanged += HandlePhaseChanged;
        }

        public override void OnNetworkDespawn()
        {
            CurrentPhase.OnValueChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase prev, GamePhase next)
        {
            OnPhaseChanged?.Invoke(next);
        }

        // ── Server-side phase advancement ────────────────────────────────────
        private void Update()
        {
            if (!IsServer) return;

            PhaseTimer.Value -= Time.deltaTime;
            if (PhaseTimer.Value <= 0f)
                AdvancePhase();
        }

        [Server]
        private void AdvancePhase()
        {
            switch (CurrentPhase.Value)
            {
                case GamePhase.Lobby:
                    SetPhase(GamePhase.Countdown, countdownDuration);
                    break;
                case GamePhase.Countdown:
                    SetPhase(GamePhase.HidePhase, hidePhaseDuration);
                    break;
                case GamePhase.HidePhase:
                    SetPhase(GamePhase.HuntPhase, huntPhaseDuration);
                    break;
                case GamePhase.HuntPhase:
                    SetPhase(GamePhase.RoundResults, resultsDuration);
                    break;
                case GamePhase.RoundResults:
                    SetPhase(GamePhase.FinalResults, resultsDuration);
                    break;
                case GamePhase.FinalResults:
                    SetPhase(GamePhase.Lobby, 0f);
                    break;
            }
        }

        [Server]
        private void SetPhase(GamePhase phase, float duration)
        {
            CurrentPhase.Value = phase;
            PhaseTimer.Value = duration;
        }

        /// <summary>Call from lobby UI once enough players are ready.</summary>
        [Server]
        public void StartGame()
        {
            if (CurrentPhase.Value != GamePhase.Lobby) return;
            SetPhase(GamePhase.Countdown, countdownDuration);
        }
    }
}
