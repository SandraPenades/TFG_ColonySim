using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ColonistRecruitment))]
[RequireComponent(typeof(AgentMovement))]
public class ThiefVisitor : MonoBehaviour
{
    private Grid mainGrid;

    [Header("Comportamiento de disimulo")]
    [SerializeField] private bool wanderBeforeStealing = true;
    [SerializeField] private float disguiseWanderRadius = 4f;
    [SerializeField] private float disguiseWanderTime = 4f;
    [SerializeField] private float disguiseMaxMoveTime = 6f;

    [Header("Robo de recursos")]
    [SerializeField] private float thiefSpeedMultiplier = 1.6f;
    [SerializeField] private float stealDistance = 1.5f;
    [SerializeField] private float stealWaitTime = 1.5f;
    [SerializeField] private float maxMoveTime = 20f;

    private ColonistRecruitment recruitment;
    private AgentMovement movement;
    private StorageBuilding targetStorage;

    private float originalSpeed;
    private bool speedApplied = false;
    private bool isRunning = false;

    public bool HasStolen { get; private set; } = false;

    [Header("Salida del ladrón")]
    public Transform exitPoint;
    private bool isLeaving = false;

    private void Awake()
    {
        recruitment = GetComponent<ColonistRecruitment>();
        movement = GetComponent<AgentMovement>();
        mainGrid = FindFirstObjectByType<Grid>();
    }

    public void StartThiefEvent()
    {
        if (isRunning) return;

        if (recruitment != null)
        {
            recruitment.SetAsVisitor();
        }

        if (movement != null)
        {
            bool snapped = movement.SnapToNavMesh();

            if (!snapped)
            {
                Debug.LogWarning("[Ladrón] No puede empezar porque no está cerca del NavMesh.");
                return;
            }
        }

        StartCoroutine(ThiefRoutine());
    }

    private IEnumerator DisguiseWanderRoutine()
    {
        if (movement == null) yield break;

        Vector2 randomCircle = Random.insideUnitCircle * disguiseWanderRadius;
        Vector3 targetPosition = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);
        targetPosition.z = transform.position.z;

        Debug.Log("[Ladrón] Disimula paseando antes de robar. Destino: " + targetPosition);

        movement.MoveTo(targetPosition);

        float timer = 0f;

        while (timer < disguiseMaxMoveTime)
        {
            timer += Time.deltaTime;

            if (recruitment != null && recruitment.IsColonyMember)
            {
                Debug.Log("[Ladrón] Ha sido reclutado durante el paseo. Se cancela el robo.");
                BecomeNormalColonist();
                yield break;
            }

            if (movement.HasReachedDestination())
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(disguiseWanderTime);
    }

    private IEnumerator ThiefRoutine()
    {
        isRunning = true;

        if (EventManager.Instance != null)
        {
            EventManager.Instance.SetThiefEventActive(true);
        }

        ApplySpeed();

        if (wanderBeforeStealing)
        {
            yield return StartCoroutine(DisguiseWanderRoutine());

            if (recruitment != null && recruitment.IsColonyMember)
            {
                yield break;
            }
        }

        targetStorage = FindStorageWithItems();

        if (targetStorage == null)
        {
            // Debug.Log("[Ladrón] No hay estantería con recursos. Se va sin robar.");
            LeaveMap();
            yield break;
        }

        Vector3 targetPosition = targetStorage.transform.position;

        if (mainGrid != null)
        {
            Vector3Int storageCell = mainGrid.WorldToCell(targetStorage.transform.position);
            targetPosition = mainGrid.GetCellCenterWorld(storageCell);
        }

        targetPosition.z = transform.position.z;

        // Debug.Log("[Ladrón] Va hacia la estantería: " + targetStorage.name + " destino " + targetPosition);

        movement.MoveTo(targetPosition);

        float timer = 0f;

        while (timer < maxMoveTime)
        {
            timer += Time.deltaTime;

            if (recruitment != null && recruitment.IsColonyMember)
            {
                Debug.Log("[Ladrón] Ha sido detenido antes de robar.");
                BecomeNormalColonist();
                yield break;
            }

            float distanceToStorage = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(targetPosition.x, targetPosition.y)
            );

            if (distanceToStorage <= stealDistance)
            {
                break;
            }

            yield return null;
        }

        float finalDistance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(targetPosition.x, targetPosition.y)
        );

        if (finalDistance > stealDistance)
        {
            // Debug.Log("[Ladrón] No ha podido llegar a la estantería. Se va sin robar.");
            LeaveMap();
            yield break;
        }

        movement.StopMoving();

        // Debug.Log("[Ladrón] Ha llegado a la estantería. Intentando robar...");

        float stealTimer = 0f;

        while (stealTimer < stealWaitTime)
        {
            stealTimer += Time.deltaTime;

            if (recruitment != null && recruitment.IsColonyMember)
            {
                // Debug.Log("[Ladrón] Ha sido detenido justo antes de robar.");
                BecomeNormalColonist();
                yield break;
            }

            yield return null;
        }

        string stolenItemID = "";
        int stolenAmount = 0;

        bool stolen = targetStorage != null && targetStorage.StealOneSlot(out stolenItemID, out stolenAmount);

        if (stolen)
        {
            HasStolen = true;
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowStolenResourceMessage(stolenItemID, stolenAmount);
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowThiefLeftWithoutStealingMessage();
            }
        }

        LeaveMap();
    }

    private StorageBuilding FindStorageWithItems()
    {
        StorageBuilding[] storages = FindObjectsByType<StorageBuilding>(FindObjectsSortMode.None);

        StorageBuilding closestStorage = null;
        float closestDistance = Mathf.Infinity;

        foreach (StorageBuilding storage in storages)
        {
            if (storage == null) continue;
            if (!storage.HasAnyItems()) continue;

            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(storage.transform.position.x, storage.transform.position.y)
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStorage = storage;
            }
        }

        return closestStorage;
    }

    private void ApplySpeed()
    {
        if (movement == null || speedApplied) return;

        originalSpeed = movement.GetSpeed();
        movement.SetSpeed(originalSpeed * thiefSpeedMultiplier);
        speedApplied = true;
    }

    private void RemoveSpeed()
    {
        if (movement == null || !speedApplied) return;

        movement.SetSpeed(originalSpeed);
        speedApplied = false;
    }

    private IEnumerator DestroyWhenLeft()
    {
        float timer = 0f;
        float maxExitTime = 12f;

        while (movement != null && !movement.HasReachedDestination())
        {
            timer += Time.deltaTime;

            if (timer >= maxExitTime)
            {
                break;
            }

            yield return null;
        }

        FinishThiefEvent();
    }

    private void FinishThiefEvent()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.SetThiefEventActive(false);
            EventManager.Instance.VisitorEventFinished();
        }

        Destroy(gameObject);
    }

    private void LeaveMap()
    {
        if (isLeaving) return;

        isLeaving = true;

        RemoveSpeed();

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
            FinishThiefEvent();
        }
    }

    private void BecomeNormalColonist()
    {
        RemoveSpeed();

        if (movement != null)
        {
            movement.StopMoving();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.SetThiefEventActive(false);
            EventManager.Instance.VisitorEventFinished();
        }

        enabled = false;
    }
}