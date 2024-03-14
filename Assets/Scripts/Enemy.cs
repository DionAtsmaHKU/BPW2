using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private CameraController cameraController;
    private TurnManager turnManager;
    public Room homeRoom;
    private bool enemyActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        cameraController = FindAnyObjectByType<CameraController>();
        turnManager = FindAnyObjectByType<TurnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraController.currentRoom != homeRoom)
        {
            enemyActivated = false;
        }

        if (IsEnemyActive())
        {
            enemyActivated = true;
            turnManager.roomEnemies.Add(this);
            transform.position += Vector3.one * Time.deltaTime;
        }
    }

    private bool IsEnemyActive()
    {
        if (cameraController.currentRoom == null || homeRoom == null)
            return false;

        if (cameraController.currentRoom == homeRoom && !enemyActivated)
            return true;

        else
            return false;
    }

    public void EnemyTurn()
    {
        Debug.Log("Enemy Turn!");
        turnManager.enemyMoves--;
    }
}
