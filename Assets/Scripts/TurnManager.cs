using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] PlayerController playerController;
    public List<Enemy> roomEnemies = new List<Enemy>();
    public int enemyMoves;
    public int playerMoves;
    public bool switchingTurn;
    public bool playerTurn;

    // Update is called once per frame
    void Update()
    {
        /*
        if (cameraController.isSwitchingScene())
        {
            roomEnemies.Clear();
        } */

        if (roomEnemies.Count == 0)
        {
            switchingTurn = true;
            playerTurn = true;
            playerMoves = 100000000;
            return;
        }

        if (!switchingTurn && playerTurn)
        {
            if (playerMoves == 0)
            {
                switchingTurn = true;
                playerTurn = false;
            }
        }

        if (!switchingTurn && !playerTurn)
        {
            if (enemyMoves == 0)
            {
                switchingTurn = true;
                playerTurn = true;
            }
        }

        if (switchingTurn && playerTurn) 
        {
            switchingTurn = false;
            playerController.turnSteps = 0;
            PlayerTurn();
        }

        if (switchingTurn && !playerTurn)
        {
            switchingTurn = false;
            EnemyTurn();
        }
    }

    private void PlayerTurn()
    {
        playerMoves = 2;
    }

    private void EnemyTurn()
    {
        enemyMoves = roomEnemies.Count;
        foreach (Enemy enemy in roomEnemies)
        {
            enemy.EnemyTurn();
        }
    }
}
