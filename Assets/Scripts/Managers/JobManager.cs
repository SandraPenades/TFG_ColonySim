using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    // Singleton para que cualquier script pueda acceder
    public static JobManager Instance;

    // La lista principal donde se guardan los trabajos
    public List<Job> pendingJobs = new List<Job>();

    void Awake()
    {
        // Configuración del singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddJob(Job.JobType type, Vector3Int position, string itemID = "")
    {
        // Para evitar duplicados
        foreach (Job existingJob in pendingJobs)
        {
            // Si el trabajo ya existe, se ignora
            if (existingJob.position == position && existingJob.type == type && existingJob.itemID == itemID && existingJob.state != Job.JobState.Completado)
            {
                return;
            }
        }

        // Crear el trabajo y guardarlo
        Job newJob = new Job(type, position, itemID);
        pendingJobs.Add(newJob);

        //Debug.Log($"[JobManager] Nuevo trabajo: {type} en {position}. Total en cola: {pendingJobs.Count}");
    }

    public Job GetNextJob(Job.JobType type, Vector3 agentPosition, string requiredItemID = "")
    {
        Job closestJob = null;
        float closestDistance = Mathf.Infinity;

        foreach (Job job in pendingJobs)
        {
            if (job.state != Job.JobState.Pendiente) continue;
            if (job.type != type) continue;

            // Si se pide un item concreto, ignora trabajos de otro item.
            // Si requiredItemID está vacío, acepta cualquier item.
            if (!string.IsNullOrEmpty(requiredItemID) && job.itemID != requiredItemID)
            {
                continue;
            }

            float distance = Vector3.Distance(agentPosition, job.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestJob = job;
            }
        }

        return closestJob;
    }

    public bool HasPendingJob(Job.JobType type, string requiredItemID = "")
    {
        foreach (Job job in pendingJobs)
        {
            if (job.type != type) continue;
            if (job.state != Job.JobState.Pendiente) continue;

            if (!string.IsNullOrEmpty(requiredItemID) && job.itemID != requiredItemID)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public int CountPendingJobs(Job.JobType type)
    {
        int count = 0;

        foreach (Job job in pendingJobs)
        {
            if (job.type == type && job.state == Job.JobState.Pendiente)
            {
                count++;
            }
        }

        return count;
    }
}
