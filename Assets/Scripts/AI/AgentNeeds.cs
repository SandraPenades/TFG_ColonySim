using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentNeeds : MonoBehaviour
{
    // Necesidades actuales
    public float hunger = 100f;
    public float energy = 100f;
    public float fun = 100f;
    public float social = 100f;

    // Desgaste por segundo
    public float hungerDecayRate = 1.5f;
    public float energyDecayRate = 1.0f;
    public float funDecayRate = 0.75f;
    public float socialDecayRate = 0.75f;

    // Umbral de alerta
    public float hungerThreshold = 30f;
    public float energyThreshold = 20f;
    public float funThreshold = 5f;
    public float socialThreshold = 5f;

    void Update()
    {
        // Primero, disminuir las necesidades con el paso del tiempo
        hunger -= hungerDecayRate * Time.deltaTime;
        energy -= energyDecayRate * Time.deltaTime;
        fun -= funDecayRate * Time.deltaTime;
        social -= socialDecayRate * Time.deltaTime;

        // Ahora ponemos un mínimo de 0 para evitar números negativos
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
        fun = Mathf.Clamp(fun, 0f, 100f);
        social = Mathf.Clamp(social, 0f, 100f);
    }

    // Funciones de aviso
    public bool IsHungry()
    {
        return hunger <= hungerThreshold;
    }

    public bool IsSleepy()
    {
        return energy <= energyThreshold;
    }

    public bool IsBored()
    {
        return fun <= funThreshold;
    }

    public bool IsLonely()
    {
        return social <= socialThreshold;
    }
}
