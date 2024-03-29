using UnityEngine;

// Contains the four directions for room doors and contains which doorType this door is.
public class Door : MonoBehaviour
{
    public enum DoorType
    {
        left, right, top, bottom
    }

    public DoorType doorType;
}
