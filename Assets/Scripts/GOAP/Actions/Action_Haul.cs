using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Haul : GoapAction
{
    public Job currentJob;
    public Vector3 targetPosition;
    public bool isDone = false;
    private GameObject targetItem;
    private StorageBuilding targetStorage;
    private bool hasStarted = false;

    [Header("Detección de recursos")]
    public LayerMask itemLayer;

    protected override void Awake()
    {
        base.Awake();

        actionName = "Transportar recurso";

        // Para el GOAP
        AddPrecondition("has_loose_resource", true);
        AddPrecondition("has_storage_available", true);
        AddEffect("resources_stored", true);
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        currentJob = JobManager.Instance.ReserveNextJob(Job.JobType.Transportar, agent.transform.position);

        if (currentJob == null)
            return false;

        Vector2 searchPos = new Vector2(currentJob.position.x, currentJob.position.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(searchPos, 1.5f, itemLayer);
        targetStorage = FindObjectOfType<StorageBuilding>();

        if (targetStorage == null)
        {
            currentJob.state = Job.JobState.Pendiente;
            currentJob = null;
            return false;
        }

        foreach (Collider2D hit in hits)
        {
            ResourceItem itemData = hit.GetComponent<ResourceItem>();
            if (itemData == null) continue;

            if (!string.IsNullOrEmpty(currentJob.itemID) && itemData.itemID != currentJob.itemID)
                continue;

            if (targetStorage.CanAcceptItem(itemData.itemID, itemData.amount))
            {
                targetItem = hit.gameObject;
                targetPosition = currentJob.position;
                return true;
            }
        }

        currentJob.state = Job.JobState.Pendiente;
        currentJob = null;
        return false;
    }

    public override void Perform(GameObject agent)
    {
        // Si no ha empezado todavía, lanzamos la corrutina
        if (!hasStarted)
        {
            if (currentJob != null) currentJob.state = Job.JobState.EnProgreso;
            StartCoroutine(HaulRoutine(agent));
            hasStarted = true;
        }
    }

    public override bool IsDone()
    {
        return isDone;
    }

    private IEnumerator HaulRoutine(GameObject agent)
    {
        AgentMovement movement = agent.GetComponent<AgentMovement>();

        // 1. Que empiece a caminar
        movement.MoveTo(targetItem.transform.position);

        float moveTimer = 0f;
        float maxMoveTime = 8f;

        // 2. Ir a por el objeto
        while (targetItem != null && !movement.HasReachedDestination())
        {
            moveTimer += Time.deltaTime;

            if (moveTimer >= maxMoveTime)
            {
                FailHaul();
                yield break;
            }

            // Debug.Log($"Pepe está a {Vector3.Distance(agent.transform.position, targetItem.transform.position)} metros del objeto");
            yield return null;
        }
        
        // Comprobamos que el objeto siga existiendo cuando estemos cerca
        if (targetItem != null)
        {
            // ¡IMPORTANTE! Le decimos al colono que deje de caminar, 
            // ya que hemos cortado el viaje a medias por estar lo bastante cerca.
            movement.StopMoving();

            // Apagamos las físicas
            Collider2D itemCol = targetItem.GetComponent<Collider2D>();
            if (itemCol != null) itemCol.enabled = false;

            // Lo ponemos en su cabeza
            targetItem.transform.SetParent(agent.transform); 
            targetItem.transform.localPosition = new Vector3(0, 0.5f, 0); 

            yield return new WaitForSeconds(0.1f);
            
            // 3. Ir a la estantería
            movement.MoveTo(targetStorage.transform.position);

            moveTimer = 0f;

            while (!movement.HasReachedDestination())
            {
                moveTimer += Time.deltaTime;

                if (moveTimer >= maxMoveTime)
                {
                    FailHaul();
                    yield break;
                }

                yield return null;
            }

            // Volvemos a pararle las piernas
            movement.StopMoving();

            // 4. Guardar en el inventario
            ResourceItem itemData = targetItem.GetComponent<ResourceItem>();

            if (itemData == null)
            {
                FailHaul();
                yield break;
            }

            int leftover = targetStorage.AddItem(itemData.itemID, itemData.amount);

            if (leftover <= 0)
            {
                Destroy(targetItem);
                targetItem = null;
            }
            else
            {
                itemData.SetAmount(leftover);

                Collider2D col = targetItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                targetItem.transform.SetParent(null);

                Vector3 currentPos = targetItem.transform.position;
                targetItem.transform.position = new Vector3(currentPos.x, currentPos.y, 0f);

                if (currentJob != null)
                {
                    currentJob.state = Job.JobState.Pendiente;
                    currentJob.position = new Vector3Int(Mathf.RoundToInt(targetItem.transform.position.x), Mathf.RoundToInt(targetItem.transform.position.y), 0);
                }

                targetItem = null;
                targetStorage = null;
                currentJob = null;
                isDone = true;
                yield break;
            }

            if (currentJob != null) 
            {
                currentJob.state = Job.JobState.Completado;
                JobManager.Instance.pendingJobs.Remove(currentJob);
                currentJob = null;
            }
        }
        else
        {
            // Si por algún motivo el objeto desapareció mientras el colono iba de camino, devolvemos el trabajo a la cola.
            if (currentJob != null) currentJob.state = Job.JobState.Pendiente;
        }
        
        isDone = true;
    }

    private void FailHaul()
    {
        if (targetItem != null)
        {
            if (targetItem.transform.parent != null)
            {
                targetItem.transform.SetParent(null);

                Vector3 currentPos = targetItem.transform.position;
                targetItem.transform.position = new Vector3(currentPos.x, currentPos.y, 0f);
            }

            Collider2D col = targetItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        if (currentJob != null)
        {
            currentJob.state = Job.JobState.Pendiente;

            if (targetItem != null)
            {
                currentJob.position = new Vector3Int(
                    Mathf.RoundToInt(targetItem.transform.position.x),
                    Mathf.RoundToInt(targetItem.transform.position.y),
                    0
                );
            }
        }

        targetItem = null;
        targetStorage = null;
        currentJob = null;
        isDone = true;
        hasStarted = false;
    }

    public override void ResetAction()
    {
        StopAllCoroutines();

        if (targetItem != null)
        {
            if (targetItem.transform.parent != null)
            {
                targetItem.transform.SetParent(null); // Para soltarlo en el mapa

                // Volver a activar el collider para que se pueda detectar
                Collider2D col = targetItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                // que la z sea 0 para que no hayan errores visuales
                Vector3 currentPos = targetItem.transform.position;
                targetItem.transform.position = new Vector3(currentPos.x, currentPos.y, 0);
            }
        }

        // Devolver el trabajo al tablón de anuncios con la nueva posición
        if (currentJob != null && currentJob.state == Job.JobState.EnProgreso)
        {
            currentJob.state = Job.JobState.Pendiente;

            if (targetItem != null)
            {
                currentJob.position = new Vector3Int(Mathf.RoundToInt(targetItem.transform.position.x), Mathf.RoundToInt(targetItem.transform.position.y), 0);
            }
        }

        targetItem = null;
        targetStorage = null;
        currentJob = null;
        isDone = false;
        hasStarted = false;
    }
}
