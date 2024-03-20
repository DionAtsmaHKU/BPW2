using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameOverUI;

    private PlayerController playerController;

    public static Action onTutorialStart;

    // GameManager singleton for easy access across the project
    private static GameManager instance;
    internal static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    private void OnEnable()
    {
        onTutorialStart += StartTutorial;
        PlayerController.onPlayerDeath += GameOver;
    }

    // Start is called before the first frame update
    void Start()
    {
       playerController = player.GetComponent<PlayerController>();
    }

    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        onTutorialStart.Invoke();
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        gameOverUI.SetActive(true);
    }

    void StartTutorial()
    {
        // RoomController.instance.OnPlayerEnterRoom(RoomController.instance.loadedRooms[1]);
        playerController.movePoint.position += new Vector3(280, 165, 0);
        player.transform.position += new Vector3(280, 165, 0);
    }
}
