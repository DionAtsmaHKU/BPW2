using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDataExtractor : MonoBehaviour
{
    [SerializeField] private Room room;
    
    private RoomController roomController;
    private RoomData roomData;

    private void OnEnable()
    {
        RoomController.OnRoomGenFinished += ProcessRooms;
    }

    private void Awake()
    {
        roomData = room.roomData;
        roomController = FindObjectOfType<RoomController>();
    }

    public void ProcessRooms()
    {
        if (roomData == null)
        {
            return;
        }

        foreach (Room room in roomController.loadedRooms)
        {
            foreach (Vector2Int tilePosition in roomData.Floor)
            {
                int neighboursCount = 4;

                // Find Wall Tiles
                if (!roomData.Floor.Contains(tilePosition + Vector2Int.up)) {
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

            }
        }
    }
}
