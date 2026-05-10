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

    public void AddJob(Job.JobType type, Vector3Int position)
    {
        // Para evitar duplicados
        foreach (Job existingJob in pendingJobs)
        {
            // Si el trabajo ya existe, se ignora
            if (existingJob.position == position && existingJob.type == type)
            {
                return;
            }
        }

        // Crear el trabajo y guardarlo
        Job newJob = new Job(type, position);
        pendingJobs.Add(newJob);

        //Debug.Log($"[JobManager] Nuevo trabajo: {type} en {position}. Total en cola: {pendingJobs.Count}");
    }

    public Job GetNextJob(Job.JobType type, Vector3 agentPosition)
    {
        Job closestJob = null;
        float closestDistance = Mathf.Infinity;

        // Buscamos en todos los trabajos pendientes
        foreach (Job job in pendingJobs)
        {
            if (job.state == Job.JobState.Pendiente && job.type == type)
            {
                // Calculamos cuál está más cerca del colono
                float distance = Vector3.Distance(agentPosition, job.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestJob = job;
                }
            }
        }

        return closestJob;
    }

    public bool HasPendingJob(Job.JobType type)
    {
        foreach (Job job in pendingJobs)
        {
            if (job.type == type && job.state == Job.JobState.Pendiente)
            {
                return true;
            }
        }

        return false;
    }
}
