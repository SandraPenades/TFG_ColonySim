using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorkPriorityRow : MonoBehaviour
{
    public WorkType workType;
    public string displayName;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI valueText;
    public Button decreaseButton;
    public Button increaseButton;

    private AgentBrain currentBrain;

    private void Awake()
    {
        if (decreaseButton != null)
        {
            decreaseButton.onClick.AddListener(DecreasePriority);
        }

        if (increaseButton != null)
        {
            increaseButton.onClick.AddListener(IncreasePriority);
        }
    }

    public void SetBrain(AgentBrain brain)
    {
        currentBrain = brain;
        Refresh();
    }

    public void Refresh()
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }

        if (valueText == null) return;

        if (currentBrain == null)
        {
            valueText.text = "-";
            return;
        }

        valueText.text = currentBrain.GetPriority(workType).ToString();
    }

    public void Clear()
    {
        currentBrain = null;

        if (valueText != null)
        {
            valueText.text = "-";
        }
    }

    private void DecreasePriority()
    {
        if (currentBrain == null) return;

        currentBrain.DecreasePriorityValue(workType);
        Refresh();
    }

    private void IncreasePriority()
    {
        if (currentBrain == null) return;

        currentBrain.IncreasePriorityValue(workType);
        Refresh();
    }
}
