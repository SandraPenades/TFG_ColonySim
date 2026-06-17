using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroLorePanel : MonoBehaviour
{
    [Header("Referencias de interfaz")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;

    [Header("Plantillas de texto")]
    [TextArea(2, 4)]
    [SerializeField] private string titleTemplate = "El comienzo de {colonyName}";
    [TextArea(6, 12)]
    [SerializeField] private string bodyTemplate = 
        "Tras días de camino, {colonist1}, {colonist2} y {colonist3} han llegado a estas tierras.\n\n" +
        "Todavía no hay hogar, ni refugio, ni un lugar donde guardar lo poco que tienen.\n\n" + 
        "Pero toda colonia empieza con una primera decisión.\n\n" + 
        "Elige dónde colocar la estantería inicial para fundar {colonyName}.";
    
    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }

    private void Start()
    {
        ShowIntro();
    }

    private void ShowIntro()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (InitialColonyPlacementManager.Instance != null)
        {
            InitialColonyPlacementManager.Instance.SetPlacementEnabled(false);
        }

        string colonyName = GetColonyName();
        string colonist1 = GetColonistName(0);
        string colonist2 = GetColonistName(1);
        string colonist3 = GetColonistName(2);

        if (titleText != null)
        {
            titleText.text = titleTemplate
                .Replace("{colonyName}", colonyName)
                .Replace("{colonist1}", colonist1)
                .Replace("{colonist2}", colonist2)
                .Replace("{colonist3}", colonist3);
        }

        if (bodyText != null)
        {
            bodyText.text = bodyTemplate
                .Replace("{colonyName}", colonyName)
                .Replace("{colonist1}", colonist1)
                .Replace("{colonist2}", colonist2)
                .Replace("{colonist3}", colonist3);
        }
    }

    private void OnContinuePressed()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (InitialColonyPlacementManager.Instance != null)
        {
            InitialColonyPlacementManager.Instance.StartInitialShelfPlacement();
        }
        else
        {
            Debug.LogWarning("[IntroLorePanel] No existe InitialColonyPlacementManager.");
        }
    }

    private string GetColonyName()
    {
        if (GameSetupData.Instance == null) return "Nueva Colonia";

        if (string.IsNullOrWhiteSpace(GameSetupData.Instance.colonyName))
        {
            return "Nueva Colonia";
        }

        return GameSetupData.Instance.colonyName;
    }

    private string GetColonistName(int index)
    {
        if (GameSetupData.Instance == null) return "un colono";

        if (GameSetupData.Instance.initialColonists == null) return "un colono";

        if (index < 0 || index >= GameSetupData.Instance.initialColonists.Count)
        {
            return "un colono";
        }

        string colonistName = GameSetupData.Instance.initialColonists[index].colonistName;

        if (string.IsNullOrWhiteSpace(colonistName))
        {
            return "un colono";
        }

        return colonistName;
    }
}
