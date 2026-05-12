using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ColonistInfoPanel : MonoBehaviour
{
    [Header("Textos del panel")]
    public TextMeshProUGUI colonistNameText;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI planText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI decisionText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI funText;
    public TextMeshProUGUI socialText;

    private GameObject selectedColonist;
    private AgentBrain brain;
    private AgentNeeds needs;

    private void Start()
    {
        ClearPanel();
    }

    private void Update()
    {
        if (selectedColonist == null) return;

        UpdatePanelInfo();
    }

    public void SetColonist(GameObject colonist)
    {
        selectedColonist = colonist;

        if (selectedColonist != null)
        {
            brain = selectedColonist.GetComponent<AgentBrain>();
            needs = selectedColonist.GetComponent<AgentNeeds>();

            gameObject.SetActive(true);
            UpdatePanelInfo();
        }
        else
        {
            ClearPanel();
        }
    }

    public void ClearPanel()
    {
        selectedColonist = null;
        brain = null;
        needs = null;

        gameObject.SetActive(false);
    }

    private void UpdatePanelInfo()
    {
        if (selectedColonist == null) return;

        if (colonistNameText != null)
        {
            colonistNameText.text = $"Colono: {selectedColonist.name}";
        }

        if (goalText != null)
        {
            goalText.text = brain != null ? $"Objetivo: {brain.GetCurrentGoalName()}" : "Objetivo: -";
        }

        if (actionText != null)
        {
            actionText.text = brain != null ? $"Acción: {brain.GetCurrentActionName()}" : "Acción: -";
        }

        if (planText != null)
        {
            planText.text = brain != null ? $"Plan restante: {brain.GetCurrentPlanDescription()}" : "Plan restante: -";
        }

        if (decisionText != null)
        {
            decisionText.text = brain != null ? $"Decisión: {brain.GetCurrentDecisionReason()}" : "Decisión: -";
        }

        if (needs != null)
        {
            if (hungerText != null)
            {
                hungerText.text = $"Hambre: {needs.hunger:0}";
            }
            if (energyText != null)
            {
                energyText.text = $"Energía: {needs.energy:0}";
            }
            if (funText != null)
            {
                funText.text = $"Diversión: {needs.fun:0}";
            }
            if (socialText != null)
            {
                socialText.text = $"Social: {needs.social:0}";
            }
        }
        else
        {
            if (hungerText != null) hungerText.text = "Hambre: -";
            if (energyText != null) energyText.text = "Energía: -";
            if (funText != null) funText.text = "Diversión: -";
            if (socialText != null) socialText.text = "Social: -";
        }
    }
}
