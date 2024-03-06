using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Direction
{
    up = 0, left = 1, down = 2, right = 3
};

public class CrawlController : MonoBehaviour
{
    public static List<Vector2Int> positionsVisited = new List<Vector2Int>();
    private static readonly Dictionary<Direction, Vector2Int> directionMovementMap = new Dictionary<Direction, Vector2Int>
    {
        {Direction.up, Vector2Int.up},
        {Direction.left, Vector2Int.left},
        {Direction.down, Vector2Int.down},
        {Direction.right, Vector2Int.right}
    };

    /* Adds a number of crawlers at 0,0 who will then use the Move function to 
     * add a new position at every location they visit for a random amount of 
     * iterations, all dependent on the GenData ScriptableObject. 
     *  Returns the positions visited as a List of Vector2Int's. */
    public static List<Vector2Int> GenerateDungeon(GenData dungeonData)
    {
        List<Crawler> dungeonCrawlers = new List<Crawler>();

        for (int i = 0; i < dungeonData.numberOfCrawlers; i++)
        {
            // make instance first?
            // GameObject go = (GameObject)GameObject.Instantiate(null);
            GameObject go = new GameObject();
            Crawler c = go.AddComponent<Crawler>();
            dungeonCrawlers.Add(c);
            c.Position = Vector2Int.zero;
        }

        int iterations = Random.Range(dungeonData.iterationMin, dungeonData.iterationMax);

        for (int i = 0; i < iterations; i++)
        {
            foreach(Crawler dungeonCrawler in dungeonCrawlers)
            {
                Vector2Int newPos = dungeonCrawler.Move(directionMovementMap);
                positionsVisited.Add(newPos);
            }
        }

        return positionsVisited;
    }
}
