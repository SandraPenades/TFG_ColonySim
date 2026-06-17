using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColonistInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI colonistNameText;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI planText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI funText;
    public TextMeshProUGUI socialText;
    public TextMeshProUGUI modeText;

    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Slider funSlider;
    [SerializeField] private Slider socialSlider;
    [SerializeField] private Slider healthSlider;

    private GameObject selectedColonist;
    private GameObject selectedIcon;

    private ColonistRecruitment recruitment;
    private AgentBrain brain;
    private AgentNeeds needs;

    private void Start()
    {
        ClearPanel();
        ConfigureSlider(hungerSlider);
        ConfigureSlider(energySlider);
        ConfigureSlider(funSlider);
        ConfigureSlider(socialSlider);
        ConfigureSlider(healthSlider);
    }

    private void Update()
    {
        if (selectedColonist == null) return;

        UpdatePanelInfo();
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    public void SetColonist(GameObject colonist)
    {
        if (selectedIcon != null)
        {
            selectedIcon.SetActive(false);
            selectedIcon = null;
        }
        
        selectedColonist = colonist;

        if (selectedColonist != null)
        {
            brain = selectedColonist.GetComponent<AgentBrain>();
            needs = selectedColonist.GetComponent<AgentNeeds>();
            recruitment = selectedColonist.GetComponent<ColonistRecruitment>();

            Transform iconTransform = selectedColonist.transform.Find("selectedColonist");
            if (iconTransform != null)
            {
                selectedIcon = iconTransform.gameObject;
                selectedIcon.SetActive(true);
            }

            gameObject.SetActive(true);
            UpdatePanelInfo();
        }
        else
        {
            selectedIcon.SetActive(false);
            ClearPanel();
        }
    }

    public void ClearPanel()
    {
        if (selectedIcon != null)
        {
            selectedIcon.SetActive(false);
            selectedIcon = null;
        }

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
            string cleanName = selectedColonist.name.Replace("Colonist_", "");
            colonistNameText.text = cleanName;
        }

        if (goalText != null)
        {
            goalText.text = brain != null ? $"{brain.GetCurrentGoalName()}" : "-";
        }

        if (actionText != null)
        {
            actionText.text = brain != null ? $"{brain.GetCurrentActionName()}" : "-";
        }

        if (planText != null)
        {
            planText.text = brain != null ? $"{brain.GetCurrentPlanDescription()}" : "-";
        }

        if (modeText != null)
        {
            if (recruitment != null && recruitment.IsRecruited)
            {
                modeText.text = "Estado: Reclutado";
            }
            else
            {
                modeText.text = "Estado: Autónomo";
            }
        }

        if (needs != null)
        {
            if (hungerText != null | hungerSlider != null)
            {
                hungerText.text = $"{needs.hunger:0}/100";
                hungerSlider.value = needs.hunger;
            }
            if (energyText != null | energySlider != null)
            {
                energyText.text = $"{needs.energy:0}/100";
                energySlider.value = needs.energy;
            }
            if (funText != null | funSlider != null)
            {
                funText.text = $"{needs.fun:0}/100";
                funSlider.value = needs.fun;
            }
            if (socialText != null | socialSlider != null)
            {
                socialText.text = $"{needs.social:0}/100";
                socialSlider.value = needs.social;
            }
            if (healthSlider != null)
            {
                healthSlider.value = needs.health;
            }
        }
        else
        {
            if (hungerText != null) 
            {
                hungerText.text = "-/100";
                hungerSlider.value = 0;
            }
            if (energyText != null) 
            {
                energyText.text = "-/100";
                energySlider.value = 0;
            }
            if (funText != null) 
            {
                funText.text = "-/100";
                funSlider.value = 0;
            }
            if (socialText != null) 
            {
                socialText.text = "-/100";
                socialSlider.value = 0;
            }
        }
    }
}
