using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;
    public InputActionReference pauseAction;
    public PlayerController playerController;

    bool isPaused = false;

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPause;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPause;
        pauseAction.action.Disable();
    }

    void Start()
    {
        pauseCanvas.SetActive(false);
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerController.SetControls(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController.SetControls(true);
    }

    public void MainMenuReturn()
    {
        SceneManager.LoadScene("MainMenu");
    }
}