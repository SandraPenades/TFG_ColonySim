using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class BuildInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI buildNameText;
    public TextMeshProUGUI resourcesText;

    public Vector2 offset = new Vector2(20f, -20f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        rectTransform.position = (Vector2)Input.mousePosition + offset;
    }

    public void Show(Blueprint blueprint)
    {
        if (blueprint == null)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        if (buildNameText != null)
        {
            buildNameText.text = blueprint.blueprintName;
        }

        if (resourcesText != null)
        {
            StringBuilder sb = new StringBuilder();

            foreach (RequiredResource resource in blueprint.requiredResources)
            {
                sb.AppendLine($"{resource.itemID} x{resource.amount}");
            }

            resourcesText.text = sb.ToString();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
