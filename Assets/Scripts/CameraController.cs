using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public Room currentRoom;
    public float moveSpeed;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        UpdatePosition();
    }

    // Updates the current position, moving to a new targetposition if that ever changes.
    private void UpdatePosition()
    {
        if (currentRoom == null) { return; }

        Vector3 targetPos = GetCamTargetPos();
        transform.position = Vector3.MoveTowards(transform.position, targetPos, 
                                                 moveSpeed * Time.deltaTime);
    }

    // Returns the new target position of the camera as a Vector3
    private Vector3 GetCamTargetPos()
    {
        if (currentRoom == null) 
        {
            return new Vector3(250, 160);
        }

        Vector2Int targetPos = currentRoom.GetRoomCentre();
        return new Vector3(targetPos.x + 1, targetPos.y + 1, -10);
    }
}
