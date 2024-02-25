using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
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

    // Returns the centre of the room as a Vector3 (with z = 0)
    public Vector3 GetRoomCentre()
    {
        return new Vector3(x * width, y * height);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            RoomController.instance.OnPlayerEnterRoom(this);
        }
    }
}
