using MechaChameleon.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MechaChameleon.UI
{
    /// <summary>
    /// Drives the Hunter in-game HUD:
    ///   - Heat level indicator (Cold → Warm → Hot)
    ///   - Round timer
    ///   - Hiders-remaining counter
    ///   - SHOOT button callback
    ///   - Aim joystick output fed back to HunterController
    /// </summary>
    public class HunterHUD : MonoBehaviour
    {
        [Header("Heat")]
        [SerializeField] private Image    heatIcon;
        [SerializeField] private TMP_Text heatLabel;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerLabel;

        [Header("Hiders Left")]
        [SerializeField] private TMP_Text hidersLeftLabel;

        [Header("Buttons")]
        [SerializeField] private Button shootButton;

        [Header("Joysticks")]
        [SerializeField] private MobileJoystick moveStick;
        [SerializeField] private MobileJoystick aimStick;

        // ── Colour tokens ─────────────────────────────────────────────────────
        private static readonly Color ColourCold = new Color(0.37f, 0.95f, 0.77f); // mint
        private static readonly Color ColourWarm = new Color(1f,  0.84f, 0.42f);   // gold
        private static readonly Color ColourHot  = new Color(1f,  0.38f, 0.38f);   // red

        private static readonly string[] HeatLabels = { "Cold", "Warm", "Hot \U0001F525" };

        private HunterController _hunter;
        private float            _timer;

        // ── Init ──────────────────────────────────────────────────────────────
        public void Bind(HunterController hunter, float roundDuration)
        {
            _hunter = hunter;
            _timer  = roundDuration;

            shootButton.onClick.AddListener(OnShootPressed);
            hunter.HeatLevel.OnValueChanged += (_, v) => RefreshHeat(v);
            RefreshHeat(hunter.HeatLevel.Value);
        }

        private void Update()
        {
            if (_hunter == null) return;

            // Timer
            _timer -= Time.deltaTime;
            if (_timer < 0f) _timer = 0f;
            int m = (int)(_timer / 60f);
            int s = (int)(_timer % 60f);
            timerLabel.text = $"{m}:{s:00}";

            // Feed joystick values into controller (owner only)
            if (_hunter.IsOwner)
            {
                // Move is handled by PlayerController's FixedUpdate via Rigidbody
                // Aim direction is updated directly
                if (aimStick.Value.sqrMagnitude > 0.01f)
                    _hunter.AimDirection.Value = aimStick.Value.normalized;
            }
        }

        // ── Heat ──────────────────────────────────────────────────────────────
        private void RefreshHeat(float heat)
        {
            // heat 0–1 maps to Cold/Warm/Hot
            int idx = heat < 0.33f ? 0 : heat < 0.66f ? 1 : 2;
            heatLabel.text = HeatLabels[idx];
            heatIcon.color = Color.Lerp(
                Color.Lerp(ColourCold, ColourWarm, heat * 3f),
                Color.Lerp(ColourWarm, ColourHot,  (heat - 0.33f) * 3f),
                heat > 0.33f ? 1f : 0f);
        }

        // ── Button callbacks ──────────────────────────────────────────────────
        private void OnShootPressed()
        {
            // Shoot is a ServerRpc on HunterController — invoke via InputValue callback
            // We call a helper that fires the same path
            _hunter?.SendMessage("OnShoot", true, SendMessageOptions.DontRequireReceiver);
        }

        public void SetHidersLeft(int count)
            => hidersLeftLabel.text = $"{count} hider{(count == 1 ? "" : "s")} left";
    }
}
