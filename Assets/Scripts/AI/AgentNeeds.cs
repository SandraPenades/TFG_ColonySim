using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentNeeds : MonoBehaviour
{
    // Necesidades actuales
    public float hunger = 100f;
    public float energy = 100f;

    // Desgaste por segundo
    public float hungerDecayRate = 1.5f;
    public float energyDecayRate = 1.0f;

    // Umbral de alerta
    public float hungerThreshold = 30f;
    public float energyThreshold = 20f;

    void Update()
    {
        // Primero, disminuir las necesidades con el paso del tiempo
        hunger -= hungerDecayRate * Time.deltaTime;
        energy -= energyDecayRate * Time.deltaTime;

        // Ahora ponemos un mínimo de 0 para evitar números negativos
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        energy = Mathf.Clamp(energy, 0f, 100f);
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

    // Funciones para recuperar necesidades (para las acciones de comer y dormir)
    public void Eat(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        Debug.Log($"[AgentNeeds] Ha comido. Hambre actual: {Mathf.RoundToInt(hunger)}/100");
    }

    public void Sleep(float amount)
    {
        energy += amount;
        energy = Mathf.Clamp(energy, 0f, 100f);
        Debug.Log($"[AgentNeeds] Ha dormido. Energía actual: {Mathf.RoundToInt(energy)}/100");
    }
}
