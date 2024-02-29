using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Linq;

public class RoomInfo
{
    public string name;
    public int x;
    public int y;
}

public class RoomController : MonoBehaviour
{
    public static RoomController instance;
    public List<Room> loadedRooms = new List<Room>();

    private string currentWorldName = "Main";
    private RoomInfo currentLoadRoomData;
    private Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();
    private Room currentRoom;

    private bool isLoadingRoom = false;
    private bool spawnedBossRoom = false;
    private bool updatedRooms = false;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // TestRooms();
    }

    // Generates rooms in a pattern
    private void TestRooms()
    {
        LoadRoom("Start", 0, 0);
        LoadRoom("Empty", 1, 0);
        LoadRoom("Empty", 2, 0);
        LoadRoom("Empty", 3, 0);
        LoadRoom("Empty", 4, 0);
        LoadRoom("Empty", 5, 0);
        LoadRoom("Empty", -1, 0);
        LoadRoom("Empty", -2, 0);
        LoadRoom("Empty", -3, 0);
        LoadRoom("Empty", -4, 0);
        LoadRoom("Empty", -5, 0);
        LoadRoom("Empty", 0, 1);
        LoadRoom("Empty", 0, 2);
        LoadRoom("Empty", 0, 3);
        LoadRoom("Empty", 0, 4);
        LoadRoom("Empty", 0, 5);
        LoadRoom("Empty", 0, -1);
        LoadRoom("Empty", 0, -2);
        LoadRoom("Empty", 0, -3);
        LoadRoom("Empty", 0, -4);
        LoadRoom("Empty", 0, -5);
        LoadRoom("Empty", -3, 1);
        LoadRoom("Empty", -3, 2);
        LoadRoom("Empty", -3, 3);
        LoadRoom("Empty", -3, 4);
        LoadRoom("Empty", 2, 1);
        LoadRoom("Empty", 2, 2);
        LoadRoom("Empty", 2, 3);
        LoadRoom("Empty", 2, 4);
        LoadRoom("Empty", 4, 1);
        LoadRoom("Empty", 4, 2);
        LoadRoom("Empty", 4, 3);
        LoadRoom("Empty", -4, -1);
        LoadRoom("Empty", -4, -2);
        LoadRoom("Empty", -4, -3);
        LoadRoom("Empty", -4, -4);
        LoadRoom("Empty", -2, -1);
        LoadRoom("Empty", -2, -2);
        LoadRoom("Empty", -2, -3);
        LoadRoom("Empty", -2, -4);
        LoadRoom("Empty", 2, -1);
        LoadRoom("Empty", 2, -2);
        LoadRoom("Empty", 2, -3);
        LoadRoom("Empty", 2, -4);
        LoadRoom("Empty", 3, -4);
        LoadRoom("Empty", 4, -4);
    }

    private void Update()
    {
        UpdateRoomQueue();
    }

    /* Loads the next room in the queueif there is one to be loaded and there 
     * currently isn't already another room being loaded. */
    private void UpdateRoomQueue()
    {
        if (isLoadingRoom)
        {
            return;
        }

        if (loadRoomQueue.Count == 0)
        {
            if (!spawnedBossRoom)
            {
                StartCoroutine(SpawnBossRoom());
            }
            else if (spawnedBossRoom && !updatedRooms)
            {
                foreach(Room room in loadedRooms)
                {
                    room.RemoveUnconnectedDoors();
                }
                updatedRooms = true;
            }
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;

        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    // IMPORTANT !!! understand soon
    IEnumerator SpawnBossRoom()
    {
        spawnedBossRoom = true;
        yield return new WaitForSeconds(0.5f);
        if (loadRoomQueue.Count == 0)
        {
            Room bossRoom = loadedRooms[loadedRooms.Count - 1];
            Room tempRoom = new Room(bossRoom.x, bossRoom.y);
            Destroy(bossRoom.gameObject);
            var roomToRemove = loadedRooms.Single(r => r.x == tempRoom.x && r.y == tempRoom.y);
            loadedRooms.Remove(roomToRemove);
            LoadRoom("End", tempRoom.x, tempRoom.y);
        }
    }

    /* Starts loading a new room and adds it to the queue, first giving it
     * a name and position */
    public void LoadRoom(string name, int x, int y)
    {
        if (DoesRoomExist(x, y))
        {
            return;
        }

        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        Debug.Log(newRoomData.name);
        newRoomData.x = x;
        newRoomData.y = y;

        loadRoomQueue.Enqueue(newRoomData);
    }

    // Assigns name to room and loads it asynchronized
    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        string roomName = currentWorldName + info.name;
        Debug.Log("Actual name: " + currentWorldName + info.name);
        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);

        while (!loadRoom.isDone)
        {
            yield return null;
        }
    }

    /* Adds the room being laoded to the list of loaded rooms, and gives it
     * all the data it needs (name and position) */
    public void RegisterRoom(Room room)
    {
        if (DoesRoomExist(currentLoadRoomData.x, currentLoadRoomData.y))
        {
            Destroy(room.gameObject);
            isLoadingRoom = false;
            return;
        }

        room.transform.position = new Vector3(currentLoadRoomData.x * room.width,
                                              currentLoadRoomData.y * room.height);

        room.x = currentLoadRoomData.x;
        room.y = currentLoadRoomData.y;
        room.name = currentWorldName + " - " + currentLoadRoomData.name + " " + room.x + " - " + room.y;
        Debug.Log("room.name = " + room.name);
        room.transform.parent = transform;

        isLoadingRoom = false;

        if (loadedRooms.Count == 0)
        {
            CameraController.instance.currentRoom = room;
        }

        loadedRooms.Add(room);
    }

    // Checks if there is a room at a certain set of coordinates
    public bool DoesRoomExist(int x, int y)
    {
        return loadedRooms.Find(item => item.x == x && item.y == y) != null; 
    }

    public Room FindRoom(int x, int y)
    {
        return loadedRooms.Find(item => item.x == x && item.y == y);
    }

    // Sets the current room for the cameracontroller to the room the player is entering
    public void OnPlayerEnterRoom(Room room)
    {
        CameraController.instance.currentRoom = room;
        currentRoom = room;
    }
}
