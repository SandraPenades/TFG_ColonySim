using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    private AgentMovement movement;

    void Start()
    {
        movement = GetComponent<AgentMovement>();
    }

    void Update()
    {
        // Al pulsar la barra espaciadora, lo mandamos a la coordenada X:0, Y:0
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 destinoPrueba = new Vector3(0, 0, 0);
            Debug.Log("Prueba de GPS: Viajando a 0,0");
            movement.MoveTo(destinoPrueba);
        }
    }
}