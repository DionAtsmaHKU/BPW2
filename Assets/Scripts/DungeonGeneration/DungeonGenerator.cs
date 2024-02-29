using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GenData genData;
    private List<Vector2Int> dungeonRooms;

    private void Start()
    {
        /* Calls Generate Dungeon, and then takes the List of positionsVisited
         * from the CrawlController to generate the dungeon. */
        dungeonRooms = CrawlController.GenerateDungeon(genData);
        SpawnRooms(dungeonRooms);
    }

    // Loads the rooms at the given locations.
    private void SpawnRooms(IEnumerable<Vector2Int> rooms)
    {
        RoomController.instance.LoadRoom("Start", 0, 0);
        foreach(Vector2Int roomLocation in rooms)
        {
            //RoomController.instance.LoadRoom("Empty", roomLocation.x, roomLocation.y); Debug.Log("case");
            
            Debug.Log("foreach reached");
            int roomType = Random.Range(0, 4);
            //int roomType = 1;
            Debug.Log(roomType);
            switch (roomType)
            {
                case 0: RoomController.instance.LoadRoom("Empty", roomLocation.x, roomLocation.y); Debug.Log("case");
                    break;
                case 1: RoomController.instance.LoadRoom("RandomA", roomLocation.x, roomLocation.y); Debug.Log("case");
                    break;
                case 2: RoomController.instance.LoadRoom("RandomB", roomLocation.x, roomLocation.y); Debug.Log("case");
                    break;
                case 3: RoomController.instance.LoadRoom("RandomC", roomLocation.x, roomLocation.y); Debug.Log("case");
                    break;
            }
            
        }
    }
}
