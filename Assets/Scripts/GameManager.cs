using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainMenuUI, gameOverUI, startImageUI, winUI;

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
        StartCoroutine(StartDelay());
    }

    public void StartGame()
    {
        
        mainMenuUI.SetActive(false);
        onTutorialStart.Invoke();
    }

    // doesn't work yet
    public void RestartGame()
    {
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
        gameOverUI.SetActive(true);
    }

    public void WinGame()
    {
        Debug.Log("Win!");
        winUI.SetActive(true);
    }

    void StartTutorial()
    {
        // RoomController.instance.OnPlayerEnterRoom(RoomController.instance.loadedRooms[1]);
        playerController.movePoint.position += new Vector3(280, 165, 0);
        player.transform.position += new Vector3(280, 165, 0);
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(4);
        startImageUI.SetActive(false);
    }
}
