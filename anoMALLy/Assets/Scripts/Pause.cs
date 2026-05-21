using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool CanPause = true;

    public GameObject pauseCanvas;
    public InputActionReference pauseAction;
    public PlayerController playerController;

    bool isPaused = false;

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPause;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPause;
            pauseAction.action.Disable();
        }
    }

    void Start()
    {
        CanPause = true;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (!CanPause) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (!CanPause) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.SetControls(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
        {
            playerController.SetControls(true);
        }
    }

    public void ForceClosePauseMenu()
    {
        isPaused = false;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    public void MainMenuReturn()
    {
        CanPause = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}