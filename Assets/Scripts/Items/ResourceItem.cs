using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceItem : MonoBehaviour
{
    // Cantidad
    public int amount = 1;

    // Componente del texto para la cantidad
    public TextMeshPro amountText;

    // Nombre del item
    public string itemID;

    public void SetAmount(int newAmount)
    {
        amount = newAmount;

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
