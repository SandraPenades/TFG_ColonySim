using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Creamos un hueco paraa el item en la estantería
[System.Serializable]
public class StorageSlot
{
    public string itemID; // comida, madera, piedra, etc.
    public int amount; // cantidad
}
public class StorageBuilding : MonoBehaviour
{
    // Inventario actual
    public List<StorageSlot> inventory = new List<StorageSlot>();

    // Configuración
    // Si está vacío, se acepta todo, si tiene alguna categoría, solo se acepta esa
    public List<string> allowedItems = new List<string>();

    // Visuales
    public TextMeshPro uiText;

    void Start()
    {
        UpdateVisuals();
    }

    // Funciones para la IA

    // Comprobar si el item está en la estantería
    public bool HasItem(string searchID)
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.Contains(searchID) && slot.amount > 0) return true;
        }
        return false;
    }

    // Coger item de la estantería
    public bool TakeItem(string searchID, int amountToTake = 1)
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.Contains(searchID))
            {
                if (slot.amount >= amountToTake) slot.amount -= amountToTake;
                else if (slot.amount < amountToTake) slot.amount = 0;
                
                UpdateVisuals();
                return true;
            }
        }
        return false; // No hay
    }

    // Actualizar el texto del item en la estantería
    private void UpdateVisuals()
    {
        if (uiText != null)
        {
            string displayText = " ";
            foreach (StorageSlot slot in inventory)
            {
                if (slot.amount > 0)
                {
                    displayText += $"{slot.itemID}: {slot.amount}\n";
                }
            }

            uiText.text = string.IsNullOrEmpty(displayText) ? "Vacío" : displayText;
        }
    }
}
