using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonistIdentity : MonoBehaviour
{
    [SerializeField] private string colonistName;

    public string ColonistName => colonistName;

    public void SetName(string newName)
    {
        colonistName = newName;
        gameObject.name = $"Colonist_{colonistName}";
    }
}
