using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Creamos un hueco para el item en la estantería
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
    public ItemDatabase database; // Archivo con todos los items y sus características
    public GameObject visualSlotPrefab; // El prefab del icono con el texto
    public Transform container; // El objeto vacío para los iconos
    public int maxSlots = 5;

    // Almacenar los sprites que ya están colocados
    private List<GameObject> spawnedVisuals = new List<GameObject>();

    void Start()
    {
        UpdateVisuals();
    }

    // Funciones para la IA

    // Comprobar si hay hueco
    public bool CanAcceptItem(string itemName)
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.ToLower() == itemName.ToLower()) return true;
        }

        return inventory.Count < maxSlots;
    }

    // Comprobar si el item está en la estantería
    public bool HasItem(string searchID)
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.Contains(searchID) && slot.amount > 0) return true;
        }
        return false;
    }

    public bool HasItemAmount(string searchID, int requiredAmount)
    {
        return GetItemAmount(searchID) >= requiredAmount;
    }

    // Guardar item en la estantería
    public void AddItem(string itemName, int amount)
    {
        // Buscamos si ya existe un hueco (slot) con este objeto
        foreach (StorageSlot slot in inventory)
        {
            // Si ya existe, le sumamos la cantidad
            if (slot.itemID.ToLower() == itemName.ToLower())
            {
                slot.amount += amount;
                UpdateVisuals();
                //Debug.Log($"Se sumaron {amount} de {itemName}. Total: {slot.amount}");
                return;
            }
        }

        if (inventory.Count >= maxSlots) return;

        StorageSlot newSlot = new StorageSlot();
        newSlot.itemID = itemName;
        newSlot.amount = amount;
        inventory.Add(newSlot);
        
        UpdateVisuals();
        // Debug.Log($"Nuevo hueco creado para: {itemName}. Cantidad inicial: {amount}");
    }

    // Coger item de la estantería
    public bool TakeItem(string searchID, int amountToTake = 1)
    {
        for (int i = 0; i < inventory.Count; i++) 
        {
            if (inventory[i].itemID.ToLower() == searchID.ToLower())
            {
                if (inventory[i].amount < amountToTake)
                {
                    return false;
                }

                inventory[i].amount -= amountToTake;

                if (inventory[i].amount <= 0)
                {
                    inventory.RemoveAt(i); 
                }
                
                UpdateVisuals();
                return true;
            }
        }
        return false;
    }

    public int GetItemAmount(string searchID)
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.ToLower() == searchID.ToLower())
            {
                return slot.amount;
            }
        }

        return 0;
    }

    private bool ConsumeRequiredResources(Blueprint blueprint)
    {
        StorageBuilding storage = FindObjectOfType<StorageBuilding>();

        if (storage == null) return false;

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            if (!storage.HasItemAmount(required.itemID, required.amount))
            {
                return false;
            }
        }

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            storage.TakeItem(required.itemID, required.amount);
        }

        return true;
    }

    // Comprobar si hay algún item comestible
    public bool HasEdibleFood(out string foodName)
    {
        foodName = "";

        foreach (StorageSlot slot in inventory)
        {
            if (slot.amount > 0 && database != null && database.IsComestible(slot.itemID))
            {
                foodName = slot.itemID;
                return true;
            }
        }
        return false;
    }

    // Actualizar los visuales de los items en la estantería
    private void UpdateVisuals()
    {
        foreach (GameObject v in spawnedVisuals)
        {
            Destroy(v);
        }
        spawnedVisuals.Clear();

        float offset = 0;

        foreach (StorageSlot slot in inventory)
        {
            if (slot.amount > 0)
            {
                Sprite s = database.GetSprite(slot.itemID);

                if (s == null) return;
                if (visualSlotPrefab == null) return;
                if (container == null) return;

                if (s != null && visualSlotPrefab != null && container != null)
                {
                    GameObject newVisual = Instantiate(visualSlotPrefab, container);

                    newVisual.transform.localPosition = new Vector3(offset, 0, 0);

                    SpriteRenderer itemSpriteRenderer = newVisual.GetComponentInChildren<SpriteRenderer>();
                    TextMeshPro amountText = newVisual.GetComponentInChildren<TextMeshPro>();

                    if (itemSpriteRenderer != null)
                    {
                        itemSpriteRenderer.sprite = s;
                        itemSpriteRenderer.sortingOrder = 10;
                    }

                    if (amountText != null)
                    {
                        amountText.text = slot.amount.ToString();

                        MeshRenderer textRenderer = amountText.GetComponent<MeshRenderer>();

                        if (textRenderer != null)
                        {
                            textRenderer.sortingOrder = 20;
                        }
                    }

                    spawnedVisuals.Add(newVisual);

                    offset += 0.3f;

                    //Debug.Log($"Icono de {slot.itemID} dibujado con éxito en la estantería.");
                }
            }
        }
    }
}
