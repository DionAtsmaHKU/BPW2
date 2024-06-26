using System.Collections.Generic;
using UnityEngine;

// Programs the movement of a single Crawler.
public class Crawler : MonoBehaviour
{
    public Vector2Int Position { get; set; }

    // Allows the crawler to move in any of 4 directions, returning a list of Positions
    public Vector2Int Move(Dictionary<Direction, Vector2Int> directionMovementMap)
    {
        Direction toMove = (Direction)Random.Range(0, directionMovementMap.Count);
        Position += directionMovementMap[toMove];
        return Position;
    }
}
