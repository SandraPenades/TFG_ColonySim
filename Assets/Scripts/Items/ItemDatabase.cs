using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseDeDatosItems", menuName = "ColonySim/Base de Datos de Items")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public string itemID;
        public Sprite sprite;
        public bool esComestible;
        // Se pueden añadir otras características como peso, valor, etc.
    }

    public List<ItemData> catalog = new List<ItemData>();

    public Sprite GetSprite(string id)
    {
        foreach (ItemData item in catalog)
        {
            if (id.ToLower().Contains(item.itemID.ToLower()))
            {
                return item.sprite;
            } 
        }
        return null;
    }

    public bool IsComestible(string id)
    {
        foreach (ItemData item in catalog)
        {
            if (id.ToLower().Contains(item.itemID.ToLower()))
            {
                return item.esComestible;
            }
        }
        return false;
    }
}
