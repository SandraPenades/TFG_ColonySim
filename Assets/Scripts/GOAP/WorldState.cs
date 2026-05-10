using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    private Dictionary<string, bool> states = new Dictionary<string, bool>();

    public void SetState(string key, bool value)
    {
        states[key] = value;
    }

    public bool HasState(string key)
    {
        return states.ContainsKey(key);
    }

    public bool GetState(string key)
    {
        return states.ContainsKey(key) && states[key];
    }

    public Dictionary<string, bool> GetStates()
    {
        return new Dictionary<string, bool>(states);
    }

    // Para poder saber si los planes funcionan, lo prueban primero en un clon
    public WorldState Clone()
    {
        WorldState clone = new WorldState();

        foreach (var state in states)
        {
            clone.SetState(state.Key, state.Value);
        }

        return clone;
    }

    // Simula una acción que ha ocurrido
    public void ApplyEffects(Dictionary<string, bool> effects)
    {
        foreach (var effect in effects)
        {
            SetState(effect.Key, effect.Value);
        }
    }

    // Comprueba si el estado actual cumple un objetivo según el plan
    public bool Satisfies(Dictionary<string, bool> desiredStates)
    {
        foreach (var desired in desiredStates)
        {
            if (!states.ContainsKey(desired.Key)) return false;
            if (states[desired.Key] != desired.Value) return false;
        }

        return true;
    }
}
