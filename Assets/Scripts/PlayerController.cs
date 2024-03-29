using System.Collections;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public static event Action onPlayerDeath;
    public static event Action onPlayerHit;

    [SerializeField] GameObject damagePopUp;
    [SerializeField] Transform canvasTransform;
    [SerializeField] AudioSource hitSound, missSound, warpSound;

    public Transform movePoint;
    public LayerMask whatStopsMovement;
    public LayerMask enemyLayer;
    
    public float moveSpeed = 5f;
    public int turnSteps;
    public bool isMovingThisTurn;
    public int hp = 20;

    public int attack = 14;
    public int defense = 8;

    public bool attackStance = true;

    private CameraController cam;
    private TurnManager turnManager;
    private int stepsPerTurn = 2;
    private bool doUpdate = true;


    private void OnEnable()
    {
        onPlayerHit += PlayerDamage;
        movePoint.parent = null;  // Disowns the movePoint
    }

    private void OnDestroy()
    {
        onPlayerHit -= PlayerDamage;
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
        if (!doUpdate)
        {
            return;
        }

        if (turnManager.switchingTurn)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                             movePoint.position, moveSpeed * Time.deltaTime);
        }

        // Possible actions on player turn
        else if (turnManager.playerTurn)
        {
            PlayerMovement();
            StanceChangeAction();
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

    // The player moves a tile. The player can move stepsPerTurn steps for one "playerMove".
    private void MoveAction()
    {
        if (turnSteps % stepsPerTurn == 0)
        {
            turnManager.playerMoves--;
        }
        else { isMovingThisTurn = true; }
    }

    /* The player attacks an enemy.
     * To do this, the player rolls an attack from 0 to 20. Then, to see if the
     * attack hits, the enemy's defense is added to the attack roll, and if this
     * result is lower than the player's attack stat, it hits. */
    private void AttackAction(Vector3 movement)
    {
        Collider2D enemyCol = Physics2D.OverlapCircle(movePoint.position
                                + movement, 0.1f, enemyLayer);
        Enemy enemy = enemyCol.GetComponentInParent<Enemy>();

        int attackRoll = UnityEngine.Random.Range(0, 20);
        if (attackRoll + enemy.enemyDef <= attack)
        {
            enemy.hp -= 10;
            SpawnPopUp(false, true, 10, true);
            turnManager.playerMoves--;
            StartCoroutine(WaitAfterAttack());
            hitSound.Play();
        }
        else 
        { 
            SpawnPopUp(false, false, 0, true);
            turnManager.playerMoves--;
            StartCoroutine(WaitAfterAttack());
            missSound.Play();
        }
    }

    /* The player changes from attack to defending stance or vice-versa.
     * This will give the player better attack or better defense. */
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
            SpawnPopUp(true, false, 0, true);
        }
    }

    // The enemy attacks the player, using the same logic from the player attack but in reverse.
    public void EnemyAttack(int enemyAtt)
    {
        int attackRoll = UnityEngine.Random.Range(0, 20);
        if (attackRoll + enemyAtt >= defense)
        {
            onPlayerHit.Invoke();
            SpawnPopUp(false, true, 10, false);
            hitSound.Play();
        }
        else 
        { 
            // Debug.Log("miss :("); 
            SpawnPopUp(false, false, 0, false);
            missSound.Play();
        }
    }

    // The player takes damage or dies if its health reaches zero.
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
            warpSound.Play();
            StartCoroutine(SetCamSpeed());
            GameManager.instance.EndTutorial();
        }
        else if (collision.CompareTag("Win"))
        {
            GameManager.Instance.WinGame();
            warpSound.Play();
        }
    }

    // Short cooldown between the player's attacks.
    IEnumerator WaitAfterAttack()
    {
        doUpdate = false;
        yield return new WaitForSeconds(0.5f);
        doUpdate = true;
    }

    IEnumerator SetCamSpeed()
    {
        yield return new WaitForSeconds(1);
        cam.moveSpeed = 25;
    }

    /* Spawns a pop-up of text, either after a player hits or misses an enemy,
     * an enemy hits or misses the player, or when the player chances stances. */
    private void SpawnPopUp(bool stanceChange, bool hit, int damage, bool byPlayer)
    {
        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(20, 40),
            UnityEngine.Random.Range(20, 40), 1);
        DamagePopUp textToSpawn = Instantiate(damagePopUp, canvasTransform).GetComponent<DamagePopUp>();
        string textToDisplay;
        textToSpawn.transform.position += randomOffset;
        if (stanceChange)
        {
            if (attackStance)
            {
                textToDisplay = "Attacking!";
            }
            else { textToDisplay = "Defending!"; }
        }
        else if (hit)
        {
            textToDisplay = "-" + damage.ToString() + " hp";
        }
        else
        {
            textToDisplay = "Miss...";
        }
        textToSpawn.DisplayText(textToDisplay, byPlayer);
    }
}
