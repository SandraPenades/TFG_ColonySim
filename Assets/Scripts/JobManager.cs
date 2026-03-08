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

        Debug.Log($"[JobManager] Nuevo trabajo: {type} en {position}. Total en cola: {pendingJobs.Count}");
    }
}
