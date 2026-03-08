using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job
{
    // Los tipos de trabajo en el juego
    public enum JobType {Talar, Minar, Recolectar, Construir}

    // En qué estado se encuentra el trabajo
    public enum JobState {Pendiente, EnProgreso, Completado}

    public JobType type;
    public JobState state;
    public Vector3Int position;

    public Job(JobType type, Vector3Int position)
    {
        this.type = type;
        this.position = position;
        this.state = JobState.Pendiente; // Por defecto
    }
}
