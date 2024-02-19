using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // Room position and dimensions
    public int width;
    public int height;
    public int x;
    public int y;

    // Start is called before the first frame update
    void Start()
    {
        if (RoomController.instance == null) 
        {
            Debug.Log("RoomController instance error");
            return;
        }

        // Registers the room when it's made
        RoomController.instance.RegisterRoom(this);
    }

    // Draws the room's bounds
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
    }

    // Returns the centre of the room
    public Vector3 GetRoomCentre()
    {
        return new Vector3(x * width, y * height);
    }
}
