using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Loss();
    }

    // Generates rooms in a funny pattern
    private void Loss()
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
        if (isLoadingRoom || loadRoomQueue.Count == 0)
        {
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;

        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
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
        newRoomData.x = x;
        newRoomData.y = y;

        loadRoomQueue.Enqueue(newRoomData);
    }

    // Assigns name to room and loads it asynchronized
    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        string roomName = currentWorldName + info.name;
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
        room.transform.position = new Vector3(currentLoadRoomData.x * room.width,
                                              currentLoadRoomData.y * room.height);

        room.x = currentLoadRoomData.x;
        room.y = currentLoadRoomData.y;
        room.name = currentWorldName + " - " + currentLoadRoomData.name + " " + room.x + " - " + room.y;
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

    // Sets the current room for the cameracontroller to the room the player is entering
    public void OnPlayerEnterRoom(Room room)
    {
        CameraController.instance.currentRoom = room;
        currentRoom = room;
    }
}
