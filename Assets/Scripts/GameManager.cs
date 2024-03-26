using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenuUI, gameOverUI, startImageUI, winUI, hpUI, tutorialUI;
    [SerializeField] private Text hpText;

    private PlayerController playerController;

    public Action onTutorialStart;

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

    // Start is called before the first frame update
    void Start()
    {
       playerController = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        hpText.text = "HP: " + playerController.hp.ToString();
    }

    public void StartGame()
    {
        hpUI.SetActive(true);
        mainMenuUI.SetActive(false);
        onTutorialStart.Invoke();
    }

    // doesn't work yet
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
    
    public void GameOver()
    {
        Debug.Log("Game Over");
        player.SetActive(false);
        gameOverUI.SetActive(true);
    }

    public void WinGame()
    {
        Debug.Log("Win!");
        player.SetActive(false);
        winUI.SetActive(true);
    }

    void StartTutorial()
    {
        // RoomController.instance.OnPlayerEnterRoom(RoomController.instance.loadedRooms[1]);
        playerController.movePoint.position += new Vector3(280, 165, 0);
        player.transform.position += new Vector3(280, 165, 0);
    }

    void LoadingScreen()
    {
        StartCoroutine(StartDelay());
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(1);
        startImageUI.SetActive(false);
    }
}
