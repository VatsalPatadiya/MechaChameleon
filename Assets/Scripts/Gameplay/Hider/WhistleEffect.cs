// WhistleEffect.cs
// Client-side: plays a brief ripple VFX + sound when the whistle RPC fires.
// Attach to the Hider prefab; HiderController calls PlayWhistleClientRpc
// which then calls this.

using System.Collections;
using UnityEngine;

namespace MechaChameleon.Gameplay.Hider
{
    public class WhistleEffect : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip   whistleClip;
        [SerializeField] private Animator    rippleAnimator; // optional particle/anim
        [SerializeField] private float       rippleDuration = 0.8f;

        public void Play()
        {
            if (audioSource && whistleClip)
                audioSource.PlayOneShot(whistleClip);

            if (rippleAnimator)
                StartCoroutine(ShowRipple());
        }

        private IEnumerator ShowRipple()
        {
            rippleAnimator.gameObject.SetActive(true);
            rippleAnimator.SetTrigger("Play");
            yield return new WaitForSeconds(rippleDuration);
            rippleAnimator.gameObject.SetActive(false);
        }
    }
}
