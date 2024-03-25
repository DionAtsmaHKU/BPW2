using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEditor.Experimental.GraphView;
using Unity.Burst.CompilerServices;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject damagePopUp;
    [SerializeField] Transform canvasTransform;

    public Transform movePoint;
    public LayerMask whatStopsMovement;
    public LayerMask enemyLayer;
    private CameraController cam;
    private TurnManager turnManager;
    public float moveSpeed = 5f;
    public int turnSteps;
    public bool isMovingThisTurn;
    private int stepsPerTurn = 2;

    public int hp = 20;
    public int attack = 14;
    public int defense = 8;

    public bool attackStance = true;

    // Action Test to get to GameOver
    public static event Action onPlayerDeath;
    public static event Action onPlayerHit;

    private void OnEnable()
    {
        onPlayerDeath += PlayerExpand;
        onPlayerHit += PlayerDamage;
        // Disowns the movePoint (I can have one joke in here right)
        movePoint.parent = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = FindAnyObjectByType<CameraController>();
        turnManager = FindAnyObjectByType<TurnManager>();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (turnManager.switchingTurn)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                             movePoint.position, moveSpeed * Time.deltaTime);
        }

        else if (turnManager.playerTurn)
        {
            PlayerMovement();
            if (!isMovingThisTurn)
            {
                StanceChangeAction();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            onPlayerDeath?.Invoke();
        }
    }

    // Handles the player's grid-based movement
    private void PlayerMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position,
                             movePoint.position, moveSpeed * Time.deltaTime);

        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"),
                                       Input.GetAxisRaw("Vertical"), 0);

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {
            if (Mathf.Abs(movement.x) == 1f && Physics2D.OverlapCircle(movePoint.position
                + new Vector3(movement.x, 0, 0), 0.1f, enemyLayer))
            {
                AttackAction(new Vector3(movement.x, 0, 0));
            }

            else if (Mathf.Abs(movement.y) == 1f && Physics2D.OverlapCircle(movePoint.position
                     + new Vector3(0, movement.y, 0), 0.1f, enemyLayer))
            {
                AttackAction(new Vector3(0, movement.y, 0));
            }

            else if (Mathf.Abs(movement.x) == 1f && !Physics2D.OverlapCircle(movePoint.position
                                + new Vector3(movement.x, 0, 0), 0.1f, whatStopsMovement))
            {
                movePoint.position += new Vector3(movement.x, 0, 0);
                turnSteps++;
                MoveAction();
            }

            else if (Mathf.Abs(movement.y) == 1f && !Physics2D.OverlapCircle(movePoint.position
                                + new Vector3(0, movement.y, 0), 0.1f, whatStopsMovement))
            {
                movePoint.position += new Vector3(0, movement.y, 0);
                turnSteps++;
                MoveAction();
            }
        }
    }

    private void MoveAction()
    {
        if (turnSteps % stepsPerTurn == 0)
        {
            isMovingThisTurn = false;
            turnManager.playerMoves--;
        }
        else { isMovingThisTurn = true; }
    }

    // attack
    private void AttackAction(Vector3 movement)
    {
        // Debug.Log("Attack enemy!");
        Collider2D enemyCol = Physics2D.OverlapCircle(movePoint.position
                                + movement, 0.1f, enemyLayer);
        // GameObject enemyObj = enemyCol.gameObject; 
        Enemy enemy = enemyCol.GetComponentInParent<Enemy>();

        int attackRoll = UnityEngine.Random.Range(0, 20);
        if (attackRoll + enemy.enemyDef <= attack)
        {
            // Debug.Log("hit!");
            enemy.hp -= 10;
            SpawnPopUp(true, 10);
            turnManager.playerMoves--;
            // PLAY SOUND
            return;
        }
        else 
        { 
            // Debug.Log("miss :(");
            SpawnPopUp(false, 0);
            turnManager.playerMoves--;
            // PLAY SOUND
        }
        
    }

    private void StanceChangeAction()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (attackStance)
            {
                attack -= 6;
                defense += 6;
                attackStance = false;
            }
            else
            {
                attack += 6;
                defense -= 6;
                attackStance = true;
            }
            turnManager.playerMoves--;
            // Do Cool Effect
        }
    }

    public void EnemyAttack(int enemyAtt)
    {
        int attackRoll = UnityEngine.Random.Range(0, 20);
        if (attackRoll + enemyAtt >= defense)
        {
            // Debug.Log("enemy hit!");
            onPlayerHit.Invoke();
            SpawnPopUp(true, 10);
            // PLAY SOUND
        }
        else 
        { 
            // Debug.Log("miss :("); 
            SpawnPopUp(false, 0);
            // PLAY SOUND
        }
    }

    void PlayerExpand()
    {
        transform.localScale *= 2;
    }

    void PlayerDamage()
    {
        hp -= 10;
        if (hp <= 0)
        {
            onPlayerDeath.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Warp"))
        {
            transform.position = Vector3.zero;
            movePoint.transform.position = Vector3.zero;
            // PLAY SOUND
            StartCoroutine(SetCamSpeed());
        }
        else if (collision.CompareTag("Win"))
        {
            GameManager.Instance.WinGame();
        }
    }

    IEnumerator SetCamSpeed()
    {
        yield return new WaitForSeconds(1);
        cam.moveSpeed = 25;
    }

    private void SpawnPopUp(bool hit, int damage)
    {
        DamagePopUp textToSpawn = Instantiate(damagePopUp, canvasTransform).GetComponent<DamagePopUp>();
        string textToDisplay;
        if (hit)
        {
            textToDisplay = "-" + damage.ToString() + " hp";
        }
        else
        {
            textToDisplay = "Miss...";
        }
        textToSpawn.DisplayText(textToDisplay);
    }
}