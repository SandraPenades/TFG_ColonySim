using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    // Velocidad de movimiento
    [SerializeField] private float speed = 3f;

    // Direccion
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        // Detectar clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetPosition();
        }

        // Moverse si no esta en el destino
        if (isMoving)
        {
            MoveCharacter();
        }
    }

    void SetTargetPosition()
    {
        // Se convierte el punto del raton a coordenadas del mundo (x, y, z) en metros
        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = 0;

        isMoving = true;
    }

    void MoveCharacter()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Si la distancia al objetivo es practicamente nula, para
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
        }
    }
}
