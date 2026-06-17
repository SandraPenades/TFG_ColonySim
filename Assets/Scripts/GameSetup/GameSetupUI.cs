using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameSetupUI : MonoBehaviour
{
    [Header("Datos de la colonia")]
    [SerializeField] private TMP_InputField colonyNameInput;
    
    [Header("Configuración de colonos")]
    [SerializeField] private ColonistSetupSlotUI[] colonistSlots;
    [SerializeField] private CharacterSkinData[] availableSkins;
    
    [Header("Escenas")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Start()
    {
        foreach (var slot in colonistSlots)
        {
            slot.Initialize(availableSkins);
        }
    }

    public void StartGame()
    {
        if (GameSetupData.Instance == null)
        {
            GameObject setupDataObject = new GameObject("GameSetupData");
            setupDataObject.AddComponent<GameSetupData>();
        }

        GameSetupData.Instance.colonyName = GetColonyName();
        GameSetupData.Instance.initialColonists.Clear();

        foreach (var slot in colonistSlots)
        {
            GameSetupData.Instance.initialColonists.Add(new ColonistSetupData
            {
                colonistName = slot.GetColonistName(),
                skin = slot.GetSelectedSkin()
            });
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private string GetColonyName()
    {
        string value = colonyNameInput.text.Trim();

        if (string.IsNullOrEmpty(value)) return "Nueva Colonia";

        return value;
    }

    public void Exit()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
