using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceItem : MonoBehaviour
{
    // Cantidad
    [Header("Cantidad del recurso")]
    public int amount = 1;

    // Componente del texto para la cantidad
    [Header("Visualización de la cantidad del recurso")]
    public TextMeshPro amountText;

    // Nombre del item
    [Header("Nombre del recurso")]
    public string itemID;

    public void SetAmount(int newAmount)
    {
        amount = newAmount;

        if (amountText == null) return;

        if (amount > 1)
        {
            amountText.text = amount.ToString();
            amountText.gameObject.SetActive(true);
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }
    }
}
