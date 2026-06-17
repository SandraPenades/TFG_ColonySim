using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubbles : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bubbleRenderer;

    [SerializeField] private Sprite[] bubbleSprites;
    [SerializeField] private float bubbleChangeTime = 1.2f;

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    private Transform target;
    private Coroutine bubbleRoutine;

    public void Initialize(Transform followTarget, Sprite[] sprites)
    {
        target = followTarget;

        if (sprites != null && sprites.Length > 0)
        {
            bubbleSprites = sprites;
        }

        CacheRenderer();
        SetSortingOrder(100);

        StartChangingBubbles();
    }

    public void InitializeFixed(Transform followTarget, Sprite sprite)
    {
        target = followTarget;

        CacheRenderer();
        SetSortingOrder(100);

        StopBubbleRoutine();

        if (bubbleRenderer != null)
        {
            bubbleRenderer.sprite = sprite;
        }
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }

    private void CacheRenderer()
    {
        if (bubbleRenderer == null)
        {
            bubbleRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void SetSortingOrder(int order)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = order;
            }
        }
    }

    private void StartChangingBubbles()
    {
        StopBubbleRoutine();
        bubbleRoutine = StartCoroutine(ChangeBubbleRoutine());
    }

    private IEnumerator ChangeBubbleRoutine()
    {
        while (true)
        {
            SetRandomBubble();

            yield return new WaitForSeconds(bubbleChangeTime);
        }
    }

    private void SetRandomBubble()
    {
        if (bubbleRenderer == null) return;
        if (bubbleSprites == null || bubbleSprites.Length == 0) return;

        int randomIndex = Random.Range(0, bubbleSprites.Length);
        bubbleRenderer.sprite = bubbleSprites[randomIndex];
    }

    private void StopBubbleRoutine()
    {
        if (bubbleRoutine != null)
        {
            StopCoroutine(bubbleRoutine);
            bubbleRoutine = null;
        }
    }

    private void OnDestroy()
    {
        StopBubbleRoutine();
    }
}