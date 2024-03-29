using UnityEngine;

[CreateAssetMenu(fileName = "GenData.asset", menuName = "GenData/Dungeon Data")]

// Contains data used for the crawlers in the dungeon generation.
public class GenData : ScriptableObject
{
    public int numberOfCrawlers;
    public int iterationMin;
    public int iterationMax;
}
