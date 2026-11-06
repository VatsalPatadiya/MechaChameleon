using UnityEngine;

namespace MechaChameleon.Gameplay
{
    /// <summary>
    /// Spawns a paint splat sprite at a world position and fades it out.
    /// Call PaintSplatVFX.Spawn() from Hunter/HiderController ClientRpcs.
    /// </summary>
    public class PaintSplatVFX : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer splatRenderer;
        [SerializeField] private float fadeDuration = 1.2f;

        private float _alpha = 1f;
        private float _elapsed;

        public static void Spawn(GameObject prefab, Vector3 position, Color colour)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
            var vfx = go.GetComponent<PaintSplatVFX>();
            if (vfx != null)
            {
                colour.a = 1f;
                vfx.splatRenderer.color = colour;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            _alpha = Mathf.Clamp01(1f - _elapsed / fadeDuration);
            var c = splatRenderer.color;
            c.a = _alpha;
            splatRenderer.color = c;
            if (_alpha <= 0f) Destroy(gameObject);
        }
    }
}
