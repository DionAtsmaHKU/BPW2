using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameOverUI;

    // private PlayerController playerController;

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
        PlayerController.onPlayerDeath += GameOver;
    }

    // Start is called before the first frame update
    void Start()
    {
        // playerController = player.GetComponent<PlayerController>();
    }

    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        player.SetActive(true);
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        gameOverUI.SetActive(true);
    }
}
