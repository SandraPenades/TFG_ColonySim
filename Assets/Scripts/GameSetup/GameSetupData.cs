using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetupData : MonoBehaviour
{
    public static GameSetupData Instance { get; private set; }

    [Header("Nombre de la colonia")]
    public string colonyName;

    [Header("Colonos iniciales")]
    public List<ColonistSetupData> initialColonists = new List<ColonistSetupData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

[System.Serializable]
public class ColonistSetupData
{
    public string colonistName;
    public CharacterSkinData skin;
}
