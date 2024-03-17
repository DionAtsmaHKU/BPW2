using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Room homeRoom;

    private CameraController cameraController;
    private TurnManager turnManager;
    private PlayerController playerController;
    private GameObject player;
    private Transform playerTransform;

    private bool enemyActivated = false;
    // private bool desynced = false;
    public int hp = 10;
    // private float moveSpeed = 5f;

    /*
    public static Action onEnemyDeath;

    private void OnEnable()
    {
        onEnemyDeath += EnemyDies;
    }

    */ 

    // Start is called before the first frame update
    void Start()
    {
        cameraController = FindAnyObjectByType<CameraController>();
        turnManager = FindAnyObjectByType<TurnManager>();

        playerController = FindAnyObjectByType<PlayerController>();
        player = playerController.gameObject;
        playerTransform = player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (hp <= 0)
        {
            Debug.Log("enemy dies !!!");
            turnManager.roomEnemies.Remove(this);
            Destroy(gameObject);
        }
        

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
        // desynced = false;
        StartCoroutine(DesyncEnemyTurns());

        /*
        Debug.Log("Enemy Turn!");
        Vector2 relativePos = (Vector2)transform.position + cameraController.currentRoom.GetRoomCentre() - 
            (Vector2)playerTransform.position + new Vector2(2, 2);
        if (relativePos.magnitude <= 1.1) // player in range
        {
            // Attack player, 
            Debug.Log("Enemy Attacks!");
            turnManager.enemyMoves--;
        }
        else
        {
            // Walk towards player
            MoveTowardsPlayer(relativePos);
            turnManager.enemyMoves--;
        }
        */
    }

    private void MoveTowardsPlayer(Vector2 relativePos)
    {
        Debug.Log(relativePos);
        if (relativePos.x >= 0 && relativePos.x >= relativePos.y)
        {
            CheckForWalls(new Vector3(-1, 0, 0));
        }

        else if (relativePos.x >= 0 && relativePos.x <= Mathf.Abs(relativePos.y))
        {
            if (relativePos.y > 0)
            {
                CheckForWalls(new Vector3(0, -1, 0));
            } else { CheckForWalls(new Vector3(0, 1, 0)); }

        }

        else if (relativePos.x < 0 && Mathf.Abs(relativePos.x) <= Mathf.Abs(relativePos.y))
        {
            if (relativePos.y > 0)
            {
                CheckForWalls(new Vector3(0, -1, 0));
            }
            else { CheckForWalls(new Vector3(0, 1, 0)); }
        }

        else
        {
            CheckForWalls(new Vector3(1, 0, 0));
        }
    }

    private void CheckForWalls(Vector3 targetPos)
    {
        Vector2 currentPos = (Vector2)transform.position + cameraController.currentRoom.GetRoomCentre() + new Vector2(2, 2);
        if (!Physics2D.OverlapCircle(currentPos + (Vector2)targetPos, 0.1f, playerController.whatStopsMovement) &&
            !Physics2D.OverlapCircle(currentPos + (Vector2)targetPos, 0.1f, playerController.enemyLayer))
        {
            transform.position += targetPos;
        }
        /*
        if (!Physics2D.OverlapCircle(transform.position + targetPos, 0.1f, playerController.whatStopsMovement) &&
            !Physics2D.OverlapCircle(transform.position + targetPos, 0.1f, playerController.enemyLayer))
        {
            transform.position += targetPos;
        }
        */
    }

    IEnumerator DesyncEnemyTurns()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 2.0f));
        // desynced = true;

        Debug.Log("Enemy Turn!");
        Vector2 relativePos = (Vector2)transform.position + cameraController.currentRoom.GetRoomCentre() -
            (Vector2)playerTransform.position + new Vector2(2, 2);
        if (relativePos.magnitude <= 1.1) // player in range
        {
            // Attack player, 
            Debug.Log("Enemy Attacks!");
            playerController.EnemyAttack();
            turnManager.enemyMoves--;
        }
        else
        {
            // Walk towards player
            MoveTowardsPlayer(relativePos);
            turnManager.enemyMoves--;
        }
    }

    /*
    private void EnemyDies()
    {
        turnManager.roomEnemies.Remove(this);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere((Vector2)transform.position + cameraController.currentRoom.GetRoomCentre(), 0.1f); ;
    }
    */
}
