using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Room homeRoom;
    public int hp = 10;
    public int enemyAtt = 2;
    public int enemyDef = 2;

    private CameraController cameraController;
    private TurnManager turnManager;
    private PlayerController playerController;
    private GameObject player;
    private Transform playerTransform;

    private bool enemyActivated = false;

    void Start()
    {
        if (tag == "TutorialEnemy")
        {
            GameManager.Instance.onTutorialStart += AddTutorialEnemy;
        }

        cameraController = FindAnyObjectByType<CameraController>();
        turnManager = FindAnyObjectByType<TurnManager>();
        playerController = FindAnyObjectByType<PlayerController>();
        player = playerController.gameObject;
        playerTransform = player.transform;
    }

    void Update()
    {
        if (hp <= 0)
        {
            /* The enemy dies if its hp reaches zero, giving the player 10 hp back, 
             * and removing the enemy from the roomEnemies List, as well as destroying it. */
            if (tag == "TutorialEnemy")
            {
                cameraController.currentRoom.tutWall.SetActive(false);
                playerController.hp = 40;
            }
            playerController.hp += 10;
            if (playerController.hp > 50) {
                playerController.hp = 50;
            }

            turnManager.roomEnemies.Remove(this);
            Destroy(gameObject);
        }
        
        // Enemies are only activated when the current room is their homeRoom.
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

    // Checks whether the enemy should be active, depending on the currentRoom and enemy homeRoom.
    private bool IsEnemyActive()
    {
        if (cameraController.currentRoom == null || homeRoom == null)
            return false;

        if (cameraController.currentRoom == homeRoom && !enemyActivated)
            return true;

        else
            return false;
    }

    /* The enemy's turn. It checks its position relative to the player, and either
     * attacks the player if it's in range, or attempts to move towards the player. */
    public void EnemyTurn()
    {
        Vector2 relativePos;

        if (homeRoom == RoomController.instance.loadedRooms[1])
        {
            relativePos = (Vector2)transform.position - (Vector2)playerTransform.position;
        } 
        else
        {
            relativePos = (Vector2)transform.position + cameraController.currentRoom.GetRoomCentre() -
                          (Vector2)playerTransform.position + new Vector2(2, 2);
        }

        if (relativePos.magnitude <= 1.1) // Player in range
        {
            // Attack player, 
            playerController.EnemyAttack(enemyAtt);
            turnManager.enemyMoves--;
        }
        else
        {
            // Walk towards player
            MoveTowardsPlayer(relativePos);
            turnManager.enemyMoves--;
        }
        
    }

    // The enemy tries to move towards the player, as long as it doesn't walk into a wall.
    private void MoveTowardsPlayer(Vector2 relativePos)
    {
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

    // The enemy checks if it is walking into a wall or a different enemy, otherwise it moves towards the targetPos.
    private void CheckForWalls(Vector3 targetPos)
    {
        Vector2 currentPos = (Vector2)transform.position + cameraController.currentRoom.GetRoomCentre() + new Vector2(2, 2);
        if (!Physics2D.OverlapCircle(currentPos + (Vector2)targetPos, 0.1f, playerController.whatStopsMovement) &&
            !Physics2D.OverlapCircle(currentPos + (Vector2)targetPos, 0.1f, playerController.enemyLayer))
        {
            transform.position += targetPos;
        }
    }

    // Adds the tutorial enemy.
    public void AddTutorialEnemy()
    {
        turnManager.roomEnemies.Add(this);
    }
}
