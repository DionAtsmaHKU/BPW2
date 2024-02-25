using UnityEngine;

[CreateAssetMenu(fileName = "GenData.asset", menuName = "GenData/Dungeon Data")]

// Contains all data used for dungeon generation.
public class GenData : ScriptableObject
{
    public int numberOfCrawlers;
    public int iterationMin;
    public int iterationMax;
}
