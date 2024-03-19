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
        // For Start and End room
        if (roomEnemies.Count == 0)
        {
            playerTurn = true;
            playerMoves = 2;
            playerController.turnSteps = 0;
            return;
        } 
 
        // Transition turn
        if (!switchingTurn && playerTurn && playerMoves <= 0)
        {
            StartCoroutine(TurnSwitchCooldown());
        }
        else if (!switchingTurn && !playerTurn && enemyMoves <= 0)
        {
            StartCoroutine(TurnSwitchCooldown());
        }
    }

    private void PlayerTurn()
    {
        playerController.turnSteps = 0;
        playerMoves = 2;
    }

    private IEnumerator EnemyTurn()
    {
        enemyMoves = roomEnemies.Count;
        foreach (Enemy enemy in roomEnemies)
        {
            enemy.EnemyTurn();
            yield return new WaitForSeconds(0.25f);
        }
    }

    // Swap turn (in 1 second)
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
