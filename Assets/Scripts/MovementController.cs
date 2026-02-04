using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    // Configuramos el NavMeshAgent
    private NavMeshAgent agent;

    void Start()
    {
        // Obtenemos la referencia del agente
        agent = GetComponent<NavMeshAgent>();

        // Para que no gire
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    void Update()
    {
        // Detectar clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            SetDestinationToMouse();
        }
    }

    void SetDestinationToMouse()
    {
        // Convertimos el raton a mundo
        Vector3 targetPos = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0;
        
        // El agente calcula la ruta
        agent.SetDestination(targetPos);
    }
}
