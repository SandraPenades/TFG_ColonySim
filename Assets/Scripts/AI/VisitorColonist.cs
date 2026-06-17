using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ColonistRecruitment))]
[RequireComponent(typeof(AgentMovement))]
public class VisitorColonist : MonoBehaviour
{
    [Header("Duración de la visita")]
    public float visitDuration = 60f;

    [Header("Movimiento libre")]
    public float wanderRadius = 5f;
    public float timeBetweenWanders = 4f;

    [Header("Comportamiento junto a la estantería")]
    public float initialDelayBeforeGoingToShelf = 1f;
    public float shelfSearchRadius = 30f;
    public float shelfWanderRadius = 3f;

    [Header("Punto de salida del visitante")]
    public Transform exitPoint;

    private ColonistRecruitment recruitment;
    private AgentMovement movement;

    private bool isLeaving = false;
    private bool eventFinished = false;
    private Coroutine visitorRoutine;

    private Transform targetShelf;

    private void Awake()
    {
        recruitment = GetComponent<ColonistRecruitment>();
        movement = GetComponent<AgentMovement>();
    }

    private void Start()
    {
        if (recruitment != null)
        {
            recruitment.SetAsVisitor();
        }

        visitorRoutine = StartCoroutine(VisitorRoutine());
    }

    private void Update()
    {
        if (recruitment != null && recruitment.IsColonyMember)
        {
            BecomeNormalColonist();
        }
    }

    public bool IsThief
    {
        get { return false; }
    }

    private IEnumerator VisitorRoutine()
    {
        float timer = 0f;

        yield return new WaitForSeconds(initialDelayBeforeGoingToShelf);

        targetShelf = FindNearestShelf();

        if (targetShelf != null && movement != null)
        {
            movement.MoveTo(targetShelf.position);

            float goToShelfTimer = 0f;
            float maxGoToShelfTime = 15f;

            while (!movement.HasReachedDestination())
            {
                if (recruitment != null && recruitment.IsColonyMember)
                {
                    BecomeNormalColonist();
                    yield break;
                }

                goToShelfTimer += Time.deltaTime;

                if (goToShelfTimer >= maxGoToShelfTime)
                {
                    break;
                }

                yield return null;
            }

            if (movement != null)
            {
                movement.StopMoving();
            }
        }

        while (timer < visitDuration)
        {
            if (recruitment != null && recruitment.IsColonyMember)
            {
                BecomeNormalColonist();
                yield break;
            }

            WanderNearShelf();

            yield return new WaitForSeconds(timeBetweenWanders);
            timer += timeBetweenWanders;
        }

        LeaveMap();
    }

    private Transform FindNearestShelf()
    {
        StorageBuilding[] storages = FindObjectsByType<StorageBuilding>(FindObjectsSortMode.None);

        Transform nearestShelf = null;
        float nearestDistance = Mathf.Infinity;

        foreach (StorageBuilding storage in storages)
        {
            if (storage == null) continue;

            float distance = Vector3.Distance(transform.position, storage.transform.position);

            if (distance < nearestDistance && distance <= shelfSearchRadius)
            {
                nearestDistance = distance;
                nearestShelf = storage.transform;
            }
        }

        return nearestShelf;
    }

    private void WanderNearShelf()
    {
        if (movement == null) return;

        Vector3 centerPosition = transform.position;

        if (targetShelf != null)
        {
            centerPosition = targetShelf.position;
        }

        Vector2 randomCircle = Random.insideUnitCircle * shelfWanderRadius;
        Vector3 targetPosition = centerPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

        movement.MoveTo(targetPosition);
    }

    private void LeaveMap()
    {
        if (isLeaving) return;

        isLeaving = true;

        if (EventManager.Instance != null)
        {
            EventManager.Instance.SetThiefEventActive(false);
        }

        if (exitPoint != null && movement != null)
        {
            movement.MoveTo(exitPoint.position);
            StartCoroutine(DestroyWhenLeft());
        }
        else
        {
            NotifyEventFinished();
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyWhenLeft()
    {
        float timer = 0f;
        float maxTime = 12f;

        while (movement != null && !movement.HasReachedDestination())
        {
            timer += Time.deltaTime;

            if (recruitment != null && recruitment.IsColonyMember)
            {
                BecomeNormalColonist();
                yield break;
            }

            if (timer >= maxTime)
            {
                NotifyEventFinished();
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        NotifyEventFinished();
        Destroy(gameObject);
    }

    private void BecomeNormalColonist()
    {
        if (eventFinished) return;

        if (EventManager.Instance != null)
        {
            EventManager.Instance.SetThiefEventActive(false);
        }

        if (visitorRoutine != null)
        {
            StopCoroutine(visitorRoutine);
            visitorRoutine = null;
        }

        if (movement != null)
        {
            movement.StopMoving();
        }

        NotifyEventFinished();

        enabled = false;
    }

    private void NotifyEventFinished()
    {
        if (eventFinished) return;

        eventFinished = true;

        if (EventManager.Instance != null)
        {
            EventManager.Instance.VisitorEventFinished();
        }
    }
}