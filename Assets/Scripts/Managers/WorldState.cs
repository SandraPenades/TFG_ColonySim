using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState : MonoBehaviour
{
    public static WorldState Instance;

    // Diccionario de condición y booleano
    private Dictionary<string, bool> states = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Comprobar el valor de una variable lógica
    public bool GetState(string key, bool value)
    {
        if (states.ContainsKey(key))
        {
            return states[key];
        }
        return false;
    }

    // Borrar una variable innecesaria
    public void RemoveState(string key)
    {
        if (states.ContainsKey(key))
        {
            states.Remove(key);
        }
    }
}
