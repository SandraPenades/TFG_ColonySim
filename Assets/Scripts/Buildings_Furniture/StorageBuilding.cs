using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Creamos un hueco para el item en la estantería
[System.Serializable]
public class StorageSlot
{
    public string itemID; 
    public int amount; 
}
public class StorageBuilding : MonoBehaviour
{
    // Inventario actual
    [Header("Inventario actual")]
    public List<StorageSlot> inventory = new List<StorageSlot>();

    // Visuales
    [Header("Base de datos de recursos")]
    public ItemDatabase database; // Archivo con todos los items y sus características
    
    [Header("Visualización del inventario")]
    public GameObject visualSlotPrefab; // El prefab del icono con el texto
    public Transform container; // El objeto vacío para los iconos
    
    [Header("Capacidad de almacenamiento")]
    public int maxSlots = 5;
    public int maxStackSize = 50;

    // Almacenar los sprites que ya están colocados
    private List<GameObject> spawnedVisuals = new List<GameObject>();

    void Start()
    {
        UpdateVisuals();
    }

    // Funciones para la IA

    // Comprobar si hay hueco
    public bool CanAcceptItem(string itemName, int amount = 1)
    {
        if (amount <= 0) return true;

        int freeSpace = 0;

        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.ToLower() == itemName.ToLower())
            {
                freeSpace += Mathf.Max(0, maxStackSize - slot.amount);
            }
        }

        int freeSlots = maxSlots - inventory.Count;
        freeSpace += freeSlots * maxStackSize;

        return freeSpace >= amount;
    }

    // Comprobar si el item está en la estantería
    public bool HasItem(string searchID)
    {
        return GetItemAmount(searchID) > 0;
    }

    public bool HasAnyItems()
    {
        foreach (StorageSlot slot in inventory)
        {
            if (slot.amount > 0) return true;
        }

        return false;
    }

    public bool HasItemAmount(string searchID, int requiredAmount)
    {
        return GetItemAmount(searchID) >= requiredAmount;
    }

    // Guardar item en la estantería
    public int AddItem(string itemName, int amount)
    {
        if (amount <= 0) return 0;

        int remainingAmount = amount;

        // Primero rellenar stacks existentes del mismo item.
        foreach (StorageSlot slot in inventory)
        {
            if (remainingAmount <= 0) break;

            if (slot.itemID.ToLower() != itemName.ToLower())
                continue;

            if (slot.amount >= maxStackSize)
                continue;

            int availableSpace = maxStackSize - slot.amount;
            int amountToAdd = Mathf.Min(availableSpace, remainingAmount);

            slot.amount += amountToAdd;
            remainingAmount -= amountToAdd;
        }

        // Después crear nuevos stacks si queda cantidad.
        while (remainingAmount > 0 && inventory.Count < maxSlots)
        {
            int amountToAdd = Mathf.Min(maxStackSize, remainingAmount);

            StorageSlot newSlot = new StorageSlot();
            newSlot.itemID = itemName;
            newSlot.amount = amountToAdd;

            inventory.Add(newSlot);

            remainingAmount -= amountToAdd;
        }

        int storedAmount = amount - remainingAmount;

        if (storedAmount > 0 && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResource(itemName, storedAmount);
        }

        UpdateVisuals();

        return remainingAmount;
    }

    // Coger item de la estantería
    public bool TakeItem(string searchID, int amountToTake = 1)
    {
        if (amountToTake <= 0) return true;

        if (GetItemAmount(searchID) < amountToTake)
        {
            return false;
        }

        int remainingAmount = amountToTake;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (remainingAmount <= 0) break;

            if (inventory[i].itemID.ToLower() != searchID.ToLower())
                continue;

            int amountTaken = Mathf.Min(inventory[i].amount, remainingAmount);

            inventory[i].amount -= amountTaken;
            remainingAmount -= amountTaken;

            if (inventory[i].amount <= 0)
            {
                inventory.RemoveAt(i);
            }
        }

        bool success = remainingAmount <= 0;

        if (success && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveResource(searchID, amountToTake);
        }

        UpdateVisuals();

        return remainingAmount <= 0;
    }

    public int GetItemAmount(string searchID)
    {
        int total = 0;

        foreach (StorageSlot slot in inventory)
        {
            if (slot.itemID.ToLower() == searchID.ToLower())
            {
                total += slot.amount;
            }
        }

        return total;
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

    public bool StealOneSlot(out string stolenItemID, out int stolenAmount)
    {
        stolenItemID = "";
        stolenAmount = 0;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].amount <= 0) continue;

            stolenItemID = inventory[i].itemID;
            stolenAmount = inventory[i].amount;

            inventory.RemoveAt(i);

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RemoveResource(stolenItemID, stolenAmount);
            }

            UpdateVisuals();

            return true;
        }

        return false;
    }
}
