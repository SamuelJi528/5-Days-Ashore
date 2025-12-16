using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject notesPanel;
    public GameObject settingsPanel;
    public GameObject deathScreenUI;
    public GameObject winPanel;

    bool isPaused;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (deathScreenUI != null && deathScreenUI.activeSelf) return;
            if (winPanel != null && winPanel.activeSelf) return;

            if (notesPanel != null && notesPanel.activeSelf)
            {
                notesPanel.SetActive(false);
                pauseMenuUI.SetActive(true);
                return;
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                pauseMenuUI.SetActive(true);
                return;
            }

            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        if (notesPanel != null) notesPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        if (notesPanel != null) notesPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenNotes()
    {
        pauseMenuUI.SetActive(false);
        if (notesPanel != null) notesPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
