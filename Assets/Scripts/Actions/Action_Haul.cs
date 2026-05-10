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
    public LayerMask itemLayer;

    public Action_Haul()
    {
        actionName = "Transportar recurso";
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        currentJob = JobManager.Instance.GetNextJob(Job.JobType.Transportar, agent.transform.position);

        if (currentJob != null)
        {
            Vector2 searchPos = new Vector2(currentJob.position.x, currentJob.position.y);
            
            // Este radar atravesará al colono y chocará con los Items.
            Collider2D hit = Physics2D.OverlapCircle(searchPos, 1.5f, itemLayer);
            
            if (hit != null)
            {
                ResourceItem itemData = hit.GetComponent<ResourceItem>();
                targetItem = hit.gameObject;
                targetStorage = FindObjectOfType<StorageBuilding>();

                if (targetStorage != null && itemData != null)
                {
                    if (targetStorage.CanAcceptItem(itemData.itemID))
                    {
                        targetItem = hit.gameObject;
                        targetPosition = currentJob.position;
                        return true;
                    }
                }
            }
        }
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
        if (targetItem != null)
        {
            movement.MoveTo(targetItem.transform.position);
        }

        // 2. Ir a por el objeto
        while (targetItem != null && Vector3.Distance(agent.transform.position, targetItem.transform.position) > 1.5f)
        {
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
            
            while (Vector3.Distance(agent.transform.position, targetStorage.transform.position) > 1.5f)
            {
                yield return null;
            }

            // Volvemos a pararle las piernas
            movement.StopMoving();

            // 4. Guardar en el inventario
            ResourceItem itemData = targetItem.GetComponent<ResourceItem>();
            string itemName = "desconocido";

            if (itemData != null)
            {
                itemName = itemData.itemID;
            }

            targetStorage.AddItem(itemName, itemData.amount);

            Destroy(targetItem);
            if (currentJob != null) currentJob.state = Job.JobState.Completado;
        }
        else
        {
            // Si por algún motivo el objeto desapareció mientras el colono iba de camino, devolvemos el trabajo a la cola.
            if (currentJob != null) currentJob.state = Job.JobState.Pendiente;
        }
        
        isDone = true;
    }

    public override void ResetAction()
    {
        StopAllCoroutines();

        if (targetItem != null)
        {
            if (targetItem.transform.parent == transform || targetItem.transform.parent != null)
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
        isDone = false;
        hasStarted = false;
    }
}
