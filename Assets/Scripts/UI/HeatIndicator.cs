// HeatIndicator.cs
// Three-segment coloured bar: Cold (blue) → Warm (orange) → Hot (red).
// Driven by HunterController.HeatLevel NetworkVariable.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MechaChameleon.Gameplay.Hunter;

namespace MechaChameleon.UI
{
    public class HeatIndicator : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI label;

        private static readonly Color Cold = new Color(0.3f, 0.6f, 1.0f);
        private static readonly Color Warm = new Color(1.0f, 0.6f, 0.2f);
        private static readonly Color Hot  = new Color(1.0f, 0.2f, 0.1f);

        public void SetHeat(float heat01)
        {
            if (fillImage) fillImage.fillAmount = heat01;

            Color col;
            string text;
            if (heat01 < 0.33f)      { col = Cold; text = "❄ Cold";  }
            else if (heat01 < 0.66f) { col = Warm; text = "🌡 Warm"; }
            else                     { col = Hot;  text = "🔥 Hot";  }

            if (fillImage) fillImage.color = col;
            if (label) label.text = text;
        }
    }
}
