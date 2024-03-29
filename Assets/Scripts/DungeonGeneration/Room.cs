using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Data about the tiles, props and enemies in the room
public class RoomData
{
    public HashSet<Vector2Int> Path { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> Floor { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesUp { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesDown { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesRight { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesLeft { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CornerTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> InnerTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> PropPositions { get; set; } = new HashSet<Vector2Int>();
    public List<GameObject> PropObjectRefrences { get; set; } = new List<GameObject>();
    public List<GameObject> EnemiesInRoom { get; set; } = new List<GameObject>();
}

// This script handles all the functions a single room uses.
public class Room : MonoBehaviour
{
    public RoomData roomData;

    public Door leftDoor, rightDoor, topDoor, bottomDoor;
    public List<Door> doors = new List<Door>();

    public int width;
    public int height;
    public int x;
    public int y;
    public Vector2Int worldOrigin;
    public GameObject tutWall;

    [SerializeField] private Tilemap floorMap, colliderMap;
    [SerializeField] private TileBase[] pathTile;
    [SerializeField] private TileBase[] floorTile;

    public Room(int temp_x, int temp_y)
    {
        x = temp_x;
        y = temp_y;

    }

    private bool updatedDoors = false;

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

        // Collecting the doors and ordering them by type.
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

    private void Update()
    {
        if (name.Contains("End") && !updatedDoors)
        {
            RemoveUnconnectedDoors();
            updatedDoors = true;
        }
    }

    // Removes all doors that lead to nowhere and adds paths between rooms using the PlacePaths function.
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
                    }
                    break;
                case Door.DoorType.left:
                    if (GetLeft() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin - new Vector2Int(width / 2, 0)));
                    }
                    break;
                case Door.DoorType.top:
                    if (GetTop() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin + new Vector2Int(0, height / 2)));
                    }
                    break;
                case Door.DoorType.bottom:
                    if (GetBottom() != null)
                    {
                        door.gameObject.SetActive(false);
                        roomData.Path.UnionWith(PlacePaths(worldOrigin, worldOrigin - new Vector2Int(0, height / 2)));
                    }
                    break;
            }
        }
    }

    // This function finds where there's supposed to be floor tiles, and places a random floor tile there.
    public HashSet<Vector2Int> PlaceFloor()
    {
        if (name.Contains("Start") || name.Contains("End") || name.Contains("Tutorial")) {
            return null;
        }

        Vector2Int roomCentre = GetRoomCentre(); 
        int leftLimit = roomCentre.x - width/2 + 2;
        int rightLimit = roomCentre.x + width/2;
        int downLimit = roomCentre.y - height/2 + 2;
        int upLimit = roomCentre.y + height/2;

        for (int i = leftLimit; i <= rightLimit; i++) 
        { 
            for (int j = downLimit; j <= upLimit; j++)
            {
                Vector2 position = new Vector2Int(i, j);
                Vector3Int positionInt = floorMap.WorldToCell(position);
                roomData.Floor.Add((Vector2Int)positionInt);
                floorMap.SetTile(positionInt, floorTile[Random.Range(0, 3)]);
            }
        }

        HashSet<Vector2Int> floorTiles = new();
        return floorTiles;
    }

    // This function places the paths between rooms and returns these paths as a HashSet of Vector2Int's.
    private HashSet<Vector2Int> PlacePaths(Vector2Int startPos, Vector2Int endPos)
    {
        // Create a hashset and add start and end positions to it
        HashSet<Vector2Int> pathTiles = new() { startPos, endPos };
        floorMap.SetTile((Vector3Int)startPos, pathTile[Random.Range(0, 3)]);
        floorMap.SetTile((Vector3Int)endPos, pathTile[Random.Range(0, 3)]);

        // Find direction of the path
        Vector2Int direction = Vector2Int.CeilToInt(((Vector2)endPos - startPos).normalized);
        Vector2Int currentPos = startPos;
        
        // Add tiles from startPos to endPos
        while (Vector2.Distance(currentPos, endPos) > 1)
        {
            currentPos += direction;
            pathTiles.Add(currentPos);
            floorMap.SetTile((Vector3Int)currentPos, null);
            floorMap.SetTile((Vector3Int)currentPos, pathTile[Random.Range(0, 3)]);
        }

        return pathTiles;
    }

    /* The next four functions find whether there is a room to the top, bottom,
     * left or right of the current room and returns that room. */

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

    // This functions finds to which HashSet each tile in the room belongs
    public void ProcessRooms()
    {
        if (roomData == null)
            return;

        foreach (Vector2Int tilePosition in roomData.Floor)
        {
            int neighboursCount = 4;

            // Find Wall Tiles
            if (!roomData.Floor.Contains(tilePosition + Vector2Int.up))
            {
                roomData.NearWallTilesUp.Add(tilePosition);
                neighboursCount--;
            }
            if (!roomData.Floor.Contains(tilePosition + Vector2Int.down))
            {
                roomData.NearWallTilesDown.Add(tilePosition);
                neighboursCount--;
            }
            if (!roomData.Floor.Contains(tilePosition + Vector2Int.right))
            {
                roomData.NearWallTilesRight.Add(tilePosition);
                neighboursCount--;
            }
            if (!roomData.Floor.Contains(tilePosition + Vector2Int.left))
            {
                roomData.NearWallTilesLeft.Add(tilePosition);
                neighboursCount--;
            }

            // Find Corner Tiles
            if (neighboursCount <= 2)
            {
                roomData.CornerTiles.Add(tilePosition);
            }
            if (neighboursCount == 4)
            {
                roomData.InnerTiles.Add(tilePosition);
            }

            roomData.NearWallTilesDown.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesUp.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesLeft.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesRight.ExceptWith(roomData.CornerTiles);
        }
    }
}
