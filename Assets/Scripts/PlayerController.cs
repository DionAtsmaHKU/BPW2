using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform movePoint;
    [SerializeField] LayerMask whatStopsMovement;
    private TurnManager turnManager;
    public float moveSpeed = 5f;
    public int turnSteps;
    public bool isMovingThisTurn;

    // Action Test to get to GameOver
    public static event Action onPlayerDeath;

    private void OnEnable()
    {
        onPlayerDeath += PlayerExpand;
    }

    // Start is called before the first frame update
    void Start()
    {
        turnManager = FindAnyObjectByType<TurnManager>();

        // Disowns the movePoint (I can have one joke in here right)
        movePoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (turnManager.playerMoves > 0)
        {
            PlayerMovement();
            if (!isMovingThisTurn)
            {
                AttackAction();
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
            if (Mathf.Abs(movement.x) == 1f && !Physics2D.OverlapCircle(movePoint.position
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
        if (turnSteps % 2 == 1)
        {
            isMovingThisTurn = true;
        }
        else if (turnSteps % 2 == 0)
        {
            turnManager.playerMoves--;
            isMovingThisTurn = false;
        }
    }

    private void AttackAction()
    {

    }

    private void StanceChangeAction()
    {

    }

    void PlayerExpand()
    {
        transform.localScale *= 2;
    }
}