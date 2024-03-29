using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages the turn-based movement and combat system.
public class TurnManager : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] PlayerController playerController;

    public List<Enemy> roomEnemies = new List<Enemy>();
    public int enemyMoves;
    public int playerMoves;
    public bool switchingTurn;
    public bool playerTurn;

    void Update()
    {
        // For Start and End room the player can move all the time.
        if (roomEnemies.Count == 0)
        {
            playerTurn = true;
            playerMoves = 2;
            playerController.turnSteps = 0;
            return;
        } 
 
        // Transition turn from player to enemy or vice versa
        if (!switchingTurn && playerTurn && playerMoves <= 0)
        {
            StartCoroutine(TurnSwitchCooldown());
        }
        else if (!switchingTurn && !playerTurn && enemyMoves <= 0)
        {
            StartCoroutine(TurnSwitchCooldown());
        }
    }

    // The player gets two moves on their turn
    private void PlayerTurn()
    {
        playerController.turnSteps = 0;
        playerMoves = 2;
    }

    // The enemies each get one move, and move independent from each other with a short delay.
    private IEnumerator EnemyTurn()
    {
        enemyMoves = roomEnemies.Count;
        foreach (Enemy enemy in roomEnemies)
        {
            enemy.EnemyTurn();
            yield return new WaitForSeconds(0.25f);
        }
    }

    // Swap turns with a 1 second delay
    public IEnumerator TurnSwitchCooldown()
    {
        switchingTurn = true;
        yield return new WaitForSeconds(1);
        playerTurn = !playerTurn;
        switchingTurn = false;

        if (playerTurn)
        { 
            PlayerTurn();
        } 
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }
}
