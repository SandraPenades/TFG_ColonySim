using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;
    public GameObject confirmExitPanel;

    private bool isPausedMenuOpen = false;

    private void Start()
    {
        CloseAllPanels();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPausedMenuOpen)
            {
                ResumeGame();
            }
            
            if (UIManager.Instance != null && UIManager.Instance.IsAnyGameplayPanelOpen())
            {
                UIManager.Instance.CloseOpenGameplayPanels();
                return;
            }

            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPausedMenuOpen = true;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);

        if (GameManager.Instance != null) GameManager.Instance.PauseGame();
    }

    public void ResumeGame()
    {
        isPausedMenuOpen = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);

        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }

    public void ConfirmExit()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(true);
    }

    public void CancelExit()
    {
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }

    public void Save()
    {
        Debug.Log("Partida Guardada");
    }

    public void Exit()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Salir sin guardar");
    }

    private void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }
}
