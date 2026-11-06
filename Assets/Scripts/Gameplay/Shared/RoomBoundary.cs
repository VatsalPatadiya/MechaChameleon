// RoomBoundary.cs
// Invisible collider walls generated procedurally from a BoxCollider2D on this
// GameObject. Hides/shows hider blend outline when they are near a wall.

using UnityEngine;

namespace MechaChameleon.Gameplay.Shared
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomBoundary : MonoBehaviour
    {
        // No runtime logic needed — Unity Physics handles blocking.
        // This component serves as a tag so the editor can identify boundary GO.
    }
}
