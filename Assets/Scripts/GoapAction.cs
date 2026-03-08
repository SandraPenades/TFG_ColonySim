using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour
{
    public string actionName = "Accion_Base";
    public float cost = 1f; // Tiempo de la acción

    public Dictionary<string, bool> preconditions;
    public Dictionary<string, bool> effects;

    public GoapAction()
    {
        preconditions = new Dictionary<string, bool>();
        effects = new Dictionary<string, bool>();
    }

    // Funciones para añadir reglas
    public void AddPrecondition(string key, bool value)
    {
        preconditions[key] = value;   
    }

    public void AddEffect(string key, bool value)
    {
        effects[key] = value;
    }

    // Funciones para cada acción

    // 1. Es físicamente posible?
    public abstract bool CheckProceduralPrecondition(GameObject agent);
    
    // 2. Qué pasa cuando el colono empieza a hacer la acción
    public abstract void Perform(GameObject agent);

    // 3. Ha terminado la acción?
    public abstract bool IsDone();

    // 4. Poner las variables a cero cuando se acaba o cancela
    public abstract void ResetAction();
}
