using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

// Contains the basic info of each room
public class RoomInfo
{
    public string name;
    public int x;
    public int y;
}

/* This script handles the loading of queueing and loading of new rooms, as well as
 * registering rooms, and containing functions to find certain rooms. */
public class RoomController : MonoBehaviour
{
    public static RoomController instance; 
    public static event Action OnRoomGenFinished;

    [SerializeField] TurnManager turnManager;

    public List<Room> loadedRooms = new List<Room>();

    private string currentWorldName = "Main";
    private RoomInfo currentLoadRoomData;
    private Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    private bool isLoadingRoom = false;
    private bool spawnedBossRoom = false;
    private bool updatedRooms = false;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        UpdateRoomQueue();
    }

    /* Loads the next room in the queue if there is one to be loaded and there 
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
                // Processing the individual rooms
                foreach (Room room in loadedRooms)
                {
                    room.PlaceFloor();
                    room.RemoveUnconnectedDoors();
                    room.ProcessRooms();
                }
                updatedRooms = true;
                OnRoomGenFinished.Invoke();
            }
            return;
        }

        // Loads the next room in the queue
        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;
        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    // Spawns the final room by replacing the room placed last in the generation with the preset End Room.
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

            // Remove placed props in the End Room
            foreach (GameObject prop in bossRoom.roomData.PropObjectRefrences)
            {
                Destroy(prop);
            }
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

    // Finds the room at coordinates (x, y).
    public Room FindRoom(int x, int y)
    {
        return loadedRooms.Find(item => item.x == x && item.y == y);
    }

    // Sets the current room for the cameracontroller to the room the player is entering
    public void OnPlayerEnterRoom(Room room)
    {
        turnManager.roomEnemies.Clear();
        CameraController.instance.currentRoom = room;
    }
}
