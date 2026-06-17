using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;

    private void Start()
    {
        Time.timeScale = 0f;
        CloseAllPanels();
    }

    public void OpenSettings()
    {
        CloseAllPanels();

        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void Play()
    {
        SceneManager.LoadScene("GameSetup");
    }

    public void Quit()
    {
        // Debug.Log("Salir");

        Application.Quit();

        #if UNITY_EDITOR
            Debug.Log("Cerrar en editor");
        #endif
    }

    private void CloseAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
