using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionThoughtIcon
{
    public string actionName;
    public Sprite bubbleSprite;
}

public class ColonistThoughtBubble : MonoBehaviour
{
    [SerializeField] private Bubbles thoughtBubblePrefab;

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.35f, 0f);

    [SerializeField] private List<ActionThoughtIcon> actionThoughtIcons = new List<ActionThoughtIcon>();

    [SerializeField] private float showTime = 2f;

    private Bubbles currentBubble;
    private float timer;

    public void ShowThought(string actionName)
    {
        Sprite sprite = GetSpriteForAction(actionName);

        if (sprite == null)
        {
            HideThought();
            return;
        }

        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
        }

        currentBubble = Instantiate(thoughtBubblePrefab);
        currentBubble.InitializeFixed(transform, sprite);
        currentBubble.SetOffset(offset);

        timer = showTime;
    }

    public void HideThought()
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
            currentBubble = null;
        }

        timer = 0f;
    }

    private void Update()
    {
        if (currentBubble == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            HideThought();
        }
    }

    private Sprite GetSpriteForAction(string actionName)
    {
        foreach (ActionThoughtIcon icon in actionThoughtIcons)
        {
            if (icon.actionName == actionName)
            {
                return icon.bubbleSprite;
            }
        }

        return null;
    }
}