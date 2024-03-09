using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomDataExtractor : MonoBehaviour
{
    [SerializeField] private Room room;
    public bool showGizmo = false;
    
    //private RoomController roomController;
    private RoomData roomData;

    private void OnEnable()
    {
        //roomController.OnRoomGenFinished += ProcessRooms;
    }

    private void Awake()
    {
        roomData = room.roomData;
        //roomController = FindObjectOfType<RoomController>();
    }

    public void ProcessRooms()
    {
        //foreach (Room room in roomController.loadedRooms)
        //{
            RoomData roomData = room.roomData;
            if (roomData == null)
                return;

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
                if (neighboursCount <= 2)
                {
                    roomData.CornerTiles.Add(tilePosition);
                }
                if (neighboursCount == 4)
                {
                    roomData.InnerTiles.Add(tilePosition);
                }
            //}

            roomData.NearWallTilesDown.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesUp.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesLeft.ExceptWith(roomData.CornerTiles);
            roomData.NearWallTilesRight.ExceptWith(roomData.CornerTiles);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //foreach (Room room in roomController.loadedRooms)
        //{
         //   RoomData roomData = room.roomData;

            if (roomData == null || !showGizmo)
                return;

            //Draw inner tiles
            Gizmos.color = Color.yellow;
            foreach (Vector2Int floorPosition in roomData.InnerTiles)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
            //Draw near wall tiles UP
            Gizmos.color = Color.blue;
            foreach (Vector2Int floorPosition in roomData.NearWallTilesUp)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
            //Draw near wall tiles DOWN
            Gizmos.color = Color.green;
            foreach (Vector2Int floorPosition in roomData.NearWallTilesDown)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
            //Draw near wall tiles RIGHT
            Gizmos.color = Color.white;
            foreach (Vector2Int floorPosition in roomData.NearWallTilesRight)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
            //Draw near wall tiles LEFT
            Gizmos.color = Color.cyan;
            foreach (Vector2Int floorPosition in roomData.NearWallTilesLeft)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
            //Draw near wall tiles CORNERS
            Gizmos.color = Color.magenta;
            foreach (Vector2Int floorPosition in roomData.CornerTiles)
            {
                if (roomData.Path.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + room.GetRoomCentre() + Vector2.one * 2, Vector2.one);
            }
        //}
    }
}
