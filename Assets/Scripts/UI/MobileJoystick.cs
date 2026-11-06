using UnityEngine;
using UnityEngine.EventSystems;

namespace MechaChameleon.UI
{
    /// <summary>
    /// Fixed-position virtual joystick for mobile.
    /// Drop onto the joystick background RectTransform.
    /// Outputs normalised direction via Value property.
    /// </summary>
    public class MobileJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 60f;

        public Vector2 Value { get; private set; }

        private RectTransform _bg;
        private Vector2 _centre;

        private void Awake() => _bg = GetComponent<RectTransform>();

        public void OnPointerDown(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _bg, e.position, e.pressEventCamera, out _centre);
            UpdateHandle(e.position, e.pressEventCamera);
        }

        public void OnDrag(PointerEventData e)
            => UpdateHandle(e.position, e.pressEventCamera);

        public void OnPointerUp(PointerEventData e)
        {
            Value = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateHandle(Vector2 screenPos, Camera cam)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _bg, screenPos, cam, out var localPoint);

            var delta = localPoint - _centre;
            Value = delta.magnitude > radius
                ? delta.normalized
                : delta / radius;

            handle.anchoredPosition = Value * radius;
        }
    }
}
