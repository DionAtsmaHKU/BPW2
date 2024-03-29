using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* This script handles which UI elements should be active at which stage of the game.
 *lso is able to restart the game by unloading all scenes and reloading Main. */
public class GameManager : MonoBehaviour
{   
    public Action onTutorialStart;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenuUI, gameOverUI, startImageUI, winUI, hpUI, tutorialUI;
    [SerializeField] private Text hpText;
    private PlayerController playerController;

    // GameManager singleton for easy access across the project
    public static GameManager instance;
    internal static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != this)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        onTutorialStart += StartTutorial;
        PlayerController.onPlayerDeath += GameOver;
        RoomController.OnRoomGenFinished += LoadingScreen;
    }

    private void OnDestroy()
    {
        PlayerController.onPlayerDeath -= GameOver;
        RoomController.OnRoomGenFinished -= LoadingScreen;
    }

    void Start()
    {
       playerController = player.GetComponent<PlayerController>();
    }

    // Updates the hpText depending on the player's current hp.
    private void Update()
    {
        hpText.text = "HP: " + playerController.hp.ToString();
    }

    // Starts the game and activates the tutorial
    public void StartGame()
    {
        hpUI.SetActive(true);
        mainMenuUI.SetActive(false);
        onTutorialStart.Invoke();
    }

    // Restarts the game by unloading all scenes and reloading Main.
    public void RestartGame()
    {
        onTutorialStart -= StartTutorial;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            SceneManager.UnloadSceneAsync(scene);
        }
        SceneManager.LoadScene("Main");
    }

    // Ends the tutorial and deactivates the tutorial UI.
    public void EndTutorial()
    {
        tutorialUI.SetActive(false);
    }

    // Activates the UI when a Game Over occurs.
    public void GameOver()
    {
        player.SetActive(false);
        gameOverUI.SetActive(true);
    }

    // Activates the UI when the player wins.
    public void WinGame()
    {
        player.SetActive(false);
        winUI.SetActive(true);
    }

    // Starts the tutorial
    void StartTutorial()
    {
        tutorialUI.SetActive(true);
        playerController.movePoint.position += new Vector3(280, 165, 0);
        player.transform.position += new Vector3(280, 165, 0);
    }

    // Starts the delay coroutine.
    void LoadingScreen()
    {
        StartCoroutine(StartDelay());
    }

    // Starts the delay between the rooms finishing loading and starting the game.
    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(1);
        startImageUI.SetActive(false);
    }
}
