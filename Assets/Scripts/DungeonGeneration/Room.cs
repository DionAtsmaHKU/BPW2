using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class RoomData
{
    public HashSet<Vector2Int> Path { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> Floor { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesUp { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesDown { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesRight { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesLeft { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CornerTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> InnterTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> PropPositions { get; set; } = new HashSet<Vector2Int>();
}

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int x;
    public int y;
    public Vector2Int worldOrigin;

    [SerializeField] private Tilemap floorMap, colliderMap;
    [SerializeField] private TileBase floorTile, pathTile;

    public RoomData roomData;

    public Room(int temp_x, int temp_y)
    {
        x = temp_x;
        y = temp_y;

    }

    public Door leftDoor, rightDoor, topDoor, bottomDoor;
    public List<Door> doors = new List<Door>();

    private bool updatedDoors = false;

    private void OnEnable()
    {
        // RoomController.OnRoomGenFinished += PlaceFloor;
    }

    private void Awake()
    {
        roomData = new RoomData();
        worldOrigin = GetRoomCentre();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (RoomController.instance == null) 
        {
            Debug.Log("RoomController instance error");
            return;
        }

        // Collecting the doors and ordering them by type. (?)
        // THIS NEXT LINE DOESN'T WORK IN MONOBEHAVIOUR
        Door[] ds = GetComponentsInChildren<Door>();
        foreach (Door d in ds)
        {
            doors.Add(d);
            switch(d.doorType)
            {
                case Door.DoorType.right:
                    rightDoor = d; break;
                case Door.DoorType.left: 
                    leftDoor = d; break;
                case Door.DoorType.top: 
                    topDoor = d; break;
                case Door.DoorType.bottom:
                    bottomDoor = d; break;
            }
        }

        // Registers the room when it's made
        RoomController.instance.RegisterRoom(this);
    }

    // Call this somehwere else
    private void Update()
    {
        if (name.Contains("End") && !updatedDoors)
        {
            RemoveUnconnectedDoors();
            updatedDoors = true;
        }
    }

    // Removes all doors that lead to nowhere.
    public void RemoveUnconnectedDoors()
    {
        foreach(Door door in doors)
        {
            switch(door.doorType)
            {
                case Door.DoorType.right:
                    if (GetRight() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin + new Vector2Int(width / 2, 0)));
                        // PlacePaths(roomCentre, roomCentre + new Vector2Int(width / 2, 0));
                    }
                    break;
                case Door.DoorType.left:
                    if (GetLeft() != null)
                    {
                        door.gameObject.SetActive(false);
                        // PlacePaths(roomCentre, roomCentre - new Vector2Int(width/2, 0));
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin - new Vector2Int(width / 2, 0)));
                    }
                    break;
                case Door.DoorType.top:
                    if (GetTop() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin + new Vector2Int(0, height / 2)));
                        // PlacePaths(roomCentre, roomCentre + new Vector2Int(0, height / 2));
                    }
                    break;
                case Door.DoorType.bottom:
                    if (GetBottom() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin - new Vector2Int(0, height / 2)));
                        // PlacePaths(roomCentre, roomCentre - new Vector2Int(0, height / 2));
                    }
                    break;
            }
        }
    }
    public HashSet<Vector2Int> PlaceFloor()
    {
        if (name.Contains("Start") || name.Contains("End")) {
            return null;
        }
        Vector2Int roomCentre = GetRoomCentre() / 2;

        int leftLimit = roomCentre.x - (width - 3)/2;
        int rightLimit = roomCentre.x + (width - 3)/2;
        int downLimit = roomCentre.y - (height - 3)/2;
        int upLimit = roomCentre.y + (height - 3)/2;

        for (int x = leftLimit; x <= rightLimit; x++) 
        { 
            for (int y = downLimit; y <= upLimit; y++)
            {
                Vector2 position = roomCentre + new Vector2Int(x, y);
                Vector3Int positionInt = floorMap.WorldToCell(position);
                roomData.Floor.Add((Vector2Int)positionInt);
                floorMap.SetTile(positionInt, floorTile);
            }
        }

        HashSet<Vector2Int> floorTiles = new();
        // Vector3Int centreInt = floorMap.WorldToCell((Vector2)roomCentre);
        // colliderMap.SetTile(centreInt, pathTile);

        return floorTiles;
    }

    private HashSet<Vector2Int> PlacePaths(Vector2Int startPos, Vector2Int endPos)
    {
        // Debug.Log(startPos);

        // Create a hashset and add start and end positions to it
        HashSet<Vector2Int> pathTiles = new() { startPos, endPos };
        floorMap.SetTile((Vector3Int)startPos, pathTile);
        floorMap.SetTile((Vector3Int)endPos, pathTile);

        // Find direction of the path
        Vector2Int direction = Vector2Int.CeilToInt(((Vector2)endPos - startPos).normalized);
        Vector2Int currentPos = startPos;
        
        // Add tiles from startPos to endPos
        while (Vector2.Distance(currentPos, endPos) > 1)
        {
            currentPos += direction;
            pathTiles.Add(currentPos);
            // Debug.Log("Placing at: " + currentPos);
            floorMap.SetTile((Vector3Int)currentPos, null);
            floorMap.SetTile((Vector3Int)currentPos, pathTile);
        }

        return pathTiles;
    }



    public Room GetRight()
    {
        if (RoomController.instance.DoesRoomExist(x + 1, y))
        {
            return RoomController.instance.FindRoom(x + 1, y);
        }
        else { return null; }
    }

    public Room GetLeft()
    {
        if (RoomController.instance.DoesRoomExist(x - 1, y))
        {
            return RoomController.instance.FindRoom(x - 1, y);
        }
        else { return null; }
    }

    public Room GetTop()
    {
        if (RoomController.instance.DoesRoomExist(x, y + 1))
        {
            return RoomController.instance.FindRoom(x, y + 1);
        }
        else { return null; }
    }

    public Room GetBottom()
    {
        if (RoomController.instance.DoesRoomExist(x, y - 1))
        {
            return RoomController.instance.FindRoom(x, y - 1);
        }
        else { return null; }
    }

    // Draws the room's bounds
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
    }

    // Returns the centre of the room as a Vector3 (with z = 0)
    public Vector2Int GetRoomCentre()
    {
        return new Vector2Int(x * width - 1, y * height - 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            RoomController.instance.OnPlayerEnterRoom(this);
        }
    }
}
