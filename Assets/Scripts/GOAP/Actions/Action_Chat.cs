using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Chat : GoapAction
{
    [Header("Recuperación social")]
    public float socialRecovered = 35f;
    public float duration = 4f;

    [Header("Movimiento hacia el colono")]
    public float conversationDistance = 1.1f;
    public float maxMoveTime = 12f;

    private AgentNeeds needs;
    private AgentMovement movement;
    private AgentNeeds targetNeeds;
    private AgentMovement targetMovement;
    private AgentBrain targetBrain;
    private GameObject targetColonist;

    private bool hasStarted = false;
    private bool isDone = false;
    private Coroutine chatCoroutine;

    [Header("Bocadillos de conversación")]
    [SerializeField] private Bubbles chatBubblePrefab;
    [SerializeField] private Sprite[] chatBubbleSprites;
    [SerializeField] private Vector3 selfBubbleOffset = new Vector3(-0.25f, 1.2f, 0f);
    [SerializeField] private Vector3 targetBubbleOffset = new Vector3(0.25f, 1.2f, 0f);

    private Bubbles selfBubble;
    private Bubbles targetBubble;

    protected override void Awake()
    {
        base.Awake();

        needs = GetComponent<AgentNeeds>();
        movement = GetComponent<AgentMovement>();

        actionName = "Socializar";

        AddPrecondition("is_lonely", true);
        AddEffect("is_socialized", true);

        cost = 2f;
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        if (needs == null) return false;
        if (needs.social >= 40f) return false;
        if (needs.IsDead) return false;

        targetColonist = FindClosestColonist();

        if (targetColonist == null) return false;

        targetNeeds = targetColonist.GetComponent<AgentNeeds>();

        return targetNeeds != null && !targetNeeds.IsDead;
    }

    private GameObject FindClosestColonist()
    {
        ColonistRecruitment[] colonists = FindObjectsByType<ColonistRecruitment>(FindObjectsSortMode.None);

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (ColonistRecruitment colonist in colonists)
        {
            if (colonist.gameObject == gameObject) continue;
            if (!colonist.IsColonyMember) continue;

            AgentNeeds otherNeeds = colonist.GetComponent<AgentNeeds>();
            if (otherNeeds == null || otherNeeds.IsDead) continue;

            float distance = Vector3.Distance(transform.position, colonist.transform.position);

            if (distance < closestDistance)
            {
                closest = colonist.gameObject;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public override bool IsDone()
    {
        return isDone || (needs != null && needs.social >= 80f);
    }

    public override void Perform(GameObject agent)
    {
        if (hasStarted) return;

        if (needs == null || targetColonist == null || targetNeeds == null || movement == null)
        {
            isDone = true;
            return;
        }

        hasStarted = true;
        isDone = false;

        StopTargetColonist();

        chatCoroutine = StartCoroutine(ChatRoutine());
    }

    private void StopTargetColonist()
    {
        if (targetColonist == null) return;

        targetMovement = targetColonist.GetComponent<AgentMovement>();
        targetBrain = targetColonist.GetComponent<AgentBrain>();

        if (targetBrain != null)
        {
            targetBrain.AbortCurrentAction();
            targetBrain.enabled = false;
        }

        if (targetMovement != null)
        {
            targetMovement.StopMoving();
        }
    }

    private void LetTargetGo()
    {
        if (targetBrain != null)
        {
            targetBrain.enabled = true;
        }

        targetBrain = null;
        targetMovement = null;
    }

    private IEnumerator ChatRoutine()
    {
        Vector3 chatPosition = GetConversationPosition();

        movement.MoveTo(chatPosition);

        float moveTimer = 0f;

        while (!movement.HasReachedDestination())
        {
            moveTimer += Time.deltaTime;

            if (moveTimer >= maxMoveTime)
            {
                HideChatBubbles();
                LetTargetGo();
                isDone = true;
                yield break;
            }

            if (needs == null || needs.IsDead)
            {
                HideChatBubbles();
                LetTargetGo();
                isDone = true;
                yield break;
            }

            if (targetColonist == null || targetNeeds == null || targetNeeds.IsDead)
            {
                HideChatBubbles();
                LetTargetGo();
                isDone = true;
                yield break;
            }

            yield return null;
        }

        movement.StopMoving();

        ShowChatBubbles();

        float timer = 0f;

        while (timer < duration)
        {
            if (needs == null || needs.IsDead)
            {
                LetTargetGo();
                isDone = true;
                yield break;
            }

            if (targetColonist == null || targetNeeds == null || targetNeeds.IsDead)
            {
                LetTargetGo();
                isDone = true;
                yield break;
            }

            float amount = (socialRecovered / duration) * Time.deltaTime;

            needs.social = Mathf.Clamp(needs.social + amount, 0f, 100f);
            targetNeeds.social = Mathf.Clamp(targetNeeds.social + amount * 0.5f, 0f, 100f);

            timer += Time.deltaTime;
            yield return null;
        }

        HideChatBubbles();
        LetTargetGo();
        isDone = true;
    }

    private Vector3 GetConversationPosition()
    {
        if (targetColonist == null)
        {
            return transform.position;
        }

        Vector3 targetPosition = targetColonist.transform.position;

        Vector3 directionFromTargetToAgent = transform.position - targetPosition;
        directionFromTargetToAgent.z = 0f;

        if (directionFromTargetToAgent.sqrMagnitude < 0.01f)
        {
            directionFromTargetToAgent = Vector3.right;
        }

        directionFromTargetToAgent.Normalize();

        Vector3 conversationPosition = targetPosition + directionFromTargetToAgent * conversationDistance;
        conversationPosition.z = targetPosition.z;

        return conversationPosition;
    }

    private void ShowChatBubbles()
    {
        if (chatBubblePrefab == null) return;

        if (selfBubble == null)
        {
            selfBubble = Instantiate(chatBubblePrefab);
            selfBubble.Initialize(transform, chatBubbleSprites);
            selfBubble.SetOffset(selfBubbleOffset);
        }

        if (targetColonist != null && targetBubble == null)
        {
            targetBubble = Instantiate(chatBubblePrefab);
            targetBubble.Initialize(targetColonist.transform, chatBubbleSprites);
            targetBubble.SetOffset(targetBubbleOffset);
        }
    }

    private void HideChatBubbles()
    {
        if (selfBubble != null)
        {
            Destroy(selfBubble.gameObject);
            selfBubble = null;
        }

        if (targetBubble != null)
        {
            Destroy(targetBubble.gameObject);
            targetBubble = null;
        }
    }

    public override void ResetAction()
    {
        if (chatCoroutine != null)
        {
            StopCoroutine(chatCoroutine);
            chatCoroutine = null;
        }

        HideChatBubbles();

        LetTargetGo();

        hasStarted = false;
        isDone = false;
        targetColonist = null;
        targetNeeds = null;
    }
}