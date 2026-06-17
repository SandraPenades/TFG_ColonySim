using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class AgentMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private ColonistAudio colonistAudio;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        colonistAudio = GetComponent<ColonistAudio>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    // Función para moverse
    public bool MoveTo(Vector3 destination)
    {
        if (agent == null)
        {
            Debug.LogWarning($"[AgentMovement] {gameObject.name} no tiene NavMeshAgent.");
            return false;
        }

        if (!agent.enabled)
        {
            Debug.LogWarning($"[AgentMovement] {gameObject.name} tiene el NavMeshAgent desactivado.");
            return false;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"[AgentMovement] {gameObject.name} NO está sobre el NavMesh.");
            return false;
        }

        if (agent.speed <= 0f)
        {
            Debug.LogWarning($"[AgentMovement] {gameObject.name} tiene speed 0.");
            return false;
        }

        destination.z = transform.position.z;

        agent.isStopped = false;
        agent.ResetPath();

        bool destinationSet = agent.SetDestination(destination);

        if (!destinationSet)
        {
            Debug.LogWarning($"[AgentMovement] {gameObject.name} no ha podido asignar destino: {destination}");
            return false;
        }

        return true;
    }

    // Función para saber si ha llegado a su destino
    public bool HasReachedDestination()
    {
        if (agent == null) return true;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance == Mathf.Infinity)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance + 0.25f)
            return false;

        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f)
            return false;

        return true;
    }

    // Función para detener al agente en seco
    public void StopMoving()
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool("IsMoving", false);
    }

    private void UpdateMovementAnimation()
    {
        if (agent == null) return;
        
        bool isMoving = agent.hasPath && !agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.1f && agent.velocity.sqrMagnitude > 0.01f;

        ColonistAudio colonistAudio = GetComponent<ColonistAudio>();

        if (colonistAudio != null)
        {
            colonistAudio.SetWalking(isMoving);
        }

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", agent.velocity.x);
        }
    }

    public float GetSpeed()
    {
        return agent.speed;
    }

    public void SetSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }

    public bool SnapToNavMesh(float maxDistance = 2f)
    {
        if (agent == null) return false;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, maxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return true;
        }

        Debug.LogWarning($"[AgentMovement] {gameObject.name} no ha encontrado NavMesh cerca del spawn.");
        return false;
    }
}
