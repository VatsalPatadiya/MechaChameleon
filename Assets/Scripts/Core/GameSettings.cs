// GameSettings.cs
// Central tunables as a ScriptableObject.
// Create via Assets → Create → MechaChameleon → GameSettings.

using UnityEngine;

namespace MechaChameleon.Core
{
    [CreateAssetMenu(menuName = "MechaChameleon/GameSettings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Phases (seconds)")]
        public float countdownDuration  = 5f;
        public float hidePhaseDuration  = 60f;
        public float huntPhaseDuration  = 120f;
        public float resultsDuration    = 10f;

        [Header("Blend")]
        [Range(0f, 10f)] public float blendDecayRate = 1.5f;  // per second while moving
        [Range(0f, 10f)] public float blendBuildRate = 3.0f;  // per second while still + painting
        [Range(0f, 100f)] public float catchThreshold = 85f;  // hunter can't catch above this

        [Header("Hunter")]
        public float shootCooldown    = 0.4f;   // seconds between shots
        public float detectionRadius  = 5f;     // BlendDetector scan radius

        [Header("Hider")]
        public int   startingWhistles = 3;
        public float whistleCooldown  = 8f;

        [Header("Roles")]
        public int playersPerHunter = 4; // 1 hunter per N players
    }
}
