using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceItem : MonoBehaviour
{
    public int amount = 1;

    public TextMeshPro amountText;

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
