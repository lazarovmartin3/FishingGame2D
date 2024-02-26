using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayUI : MonoBehaviour
{
    [SerializeField] private Button resumeBtn, mainMenuBtn;
    [SerializeField] private GameObject pauseGameMenu;
    [SerializeField] private GameObject mainMenu;

    private void Start()
    {
        resumeBtn.onClick.AddListener(ResumeGame);
        mainMenuBtn.onClick.AddListener(BackToMainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        pauseGameMenu.SetActive(true);
    }

    private void ResumeGame()
    {
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void BackToMainMenu()
    {
        //NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(0);
    }
}