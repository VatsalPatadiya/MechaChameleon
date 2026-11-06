using MechaChameleon.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MechaChameleon.UI
{
    /// <summary>
    /// Drives the Hider in-game HUD:
    ///   - Blend % bar + badge
    ///   - Round timer
    ///   - Whistle charge badge
    ///   - "Seeker nearby" warning flash
    ///   - PAINT / WATCH / WHISTLE button callbacks
    /// </summary>
    public class HiderHUD : MonoBehaviour
    {
        [Header("Blend")]
        [SerializeField] private Slider   blendSlider;
        [SerializeField] private TMP_Text blendLabel;
        [SerializeField] private Image    blendFill;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerLabel;

        [Header("Whistle")]
        [SerializeField] private TMP_Text whistleChargeLabel;
        [SerializeField] private Button   whistleButton;

        [Header("Seeker Warning")]
        [SerializeField] private GameObject seekerWarning;

        [Header("Action Buttons")]
        [SerializeField] private Button paintButton;
        [SerializeField] private Button watchButton;

        // ── Colour tokens ─────────────────────────────────────────────────────
        private static readonly Color ColourLow  = new Color(1f,  0.38f, 0.38f); // red
        private static readonly Color ColourMid  = new Color(1f,  0.84f, 0.42f); // gold
        private static readonly Color ColourHigh = new Color(0.37f,0.95f,0.77f); // mint

        private HiderController _hider;
        private float           _timer;
        private bool            _isSeekerNearby;

        // ── Init ─────────────────────────────────────────────────────────────
        public void Bind(HiderController hider, float roundDuration)
        {
            _hider = hider;
            _timer = roundDuration;

            paintButton.onClick.AddListener(OnPaintPressed);
            watchButton.onClick.AddListener(OnWatchPressed);
            whistleButton.onClick.AddListener(OnWhistlePressed);

            hider.BlendPercent.OnValueChanged  += (_, v) => RefreshBlend(v);
            hider.WhistleCharges.OnValueChanged += (_, v) => RefreshWhistle(v);

            RefreshBlend(hider.BlendPercent.Value);
            RefreshWhistle(hider.WhistleCharges.Value);
        }

        private void Update()
        {
            if (_hider == null) return;

            // Countdown timer
            _timer -= Time.deltaTime;
            if (_timer < 0f) _timer = 0f;
            int m = (int)(_timer / 60f);
            int s = (int)(_timer % 60f);
            timerLabel.text = $"{m}:{s:00}";

            // Seeker warning pulse (driven externally via ShowSeekerWarning)
        }

        // ── Blend ─────────────────────────────────────────────────────────────
        private void RefreshBlend(float value)
        {
            float t = value / 100f;
            blendSlider.value = t;
            blendLabel.text   = $"{Mathf.RoundToInt(value)}%";
            blendFill.color   = Color.Lerp(Color.Lerp(ColourLow, ColourMid, t * 2f),
                                           Color.Lerp(ColourMid, ColourHigh, (t - 0.5f) * 2f),
                                           t > 0.5f ? 1f : 0f);
        }

        // ── Whistle ───────────────────────────────────────────────────────────
        private void RefreshWhistle(int charges)
        {
            whistleChargeLabel.text = charges.ToString();
            whistleButton.interactable = charges > 0;
        }

        // ── Button callbacks ──────────────────────────────────────────────────
        private void OnPaintPressed()   => _hider?.StartPaintingServerRpc();
        private void OnWatchPressed()   { /* TODO: open spectator cam */ }
        private void OnWhistlePressed() => _hider?.UseWhistleServerRpc();

        // ── Seeker warning ────────────────────────────────────────────────────
        public void ShowSeekerWarning(bool show) => seekerWarning.SetActive(show);
    }
}
