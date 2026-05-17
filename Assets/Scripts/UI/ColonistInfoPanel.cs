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
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI handItemText;

    private GameObject selectedColonist;
    private ColonistRecruitment recruitment;
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

        recruitment = selectedColonist.GetComponent<ColonistRecruitment>();
    }

    public void ClearPanel()
    {
        selectedColonist = null;
        brain = null;
        needs = null;
        recruitment = null;

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
            goalText.text = brain != null ? $"Objetivo: \n{brain.GetCurrentGoalName()}" : "Objetivo: \n-";
        }

        if (actionText != null)
        {
            actionText.text = brain != null ? $"Acción: \n{brain.GetCurrentActionName()}" : "Acción: \n-";
        }

        if (planText != null)
        {
            planText.text = brain != null ? $"Plan restante: \n{brain.GetCurrentPlanDescription()}" : "Plan restante: \n-";
        }

        if (decisionText != null)
        {
            decisionText.text = brain != null ? $"Decisión: \n{brain.GetCurrentDecisionReason()}" : "Decisión: \n-";
        }

        if (modeText != null)
        {
            if (recruitment != null && recruitment.IsRecruited)
            {
                modeText.text = "Reclutado";
            }
            else
            {
                modeText.text = "Autónomo";
            }
        }

        if (handItemText != null)
        {
            ResourceItem carriedItem = selectedColonist.GetComponentInChildren<ResourceItem>();

            if (carriedItem != null)
            {
                handItemText.text = $"{carriedItem.itemID} x{carriedItem.amount}";
            }
            else
            {
                handItemText.text = "Nada";
            }
        }

        if (needs != null)
        {
            if (hungerText != null)
            {
                hungerText.text = $"{needs.hunger:0}";
            }
            if (energyText != null)
            {
                energyText.text = $"{needs.energy:0}";
            }
            if (funText != null)
            {
                funText.text = $"{needs.fun:0}";
            }
            if (socialText != null)
            {
                socialText.text = $"{needs.social:0}";
            }
        }
        else
        {
            if (hungerText != null) hungerText.text = "-";
            if (energyText != null) energyText.text = "-";
            if (funText != null) funText.text = "-";
            if (socialText != null) socialText.text = "-";
        }
    }
}
