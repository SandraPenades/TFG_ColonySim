using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkPriorityPanel : MonoBehaviour
{
    public WorkPriorityRow[] rows;

    private AgentBrain currentBrain;

    public void SetColonist(GameObject colonist)
    {
        if (colonist == null)
        {
            ClearPanel();
            return;
        }

        currentBrain = colonist.GetComponent<AgentBrain>();

        foreach (WorkPriorityRow row in rows)
        {
            if (row != null)
            {
                row.SetBrain(currentBrain);
            }
        }

        gameObject.SetActive(currentBrain != null);
    }

    public void ClearPanel()
    {
        currentBrain = null;

        foreach (WorkPriorityRow row in rows)
        {
            if (row != null)
            {
                row.Clear();
            }
        }

        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        foreach (WorkPriorityRow row in rows)
        {
            if (row != null)
            {
                row.Refresh();
            }
        }
    }
}
