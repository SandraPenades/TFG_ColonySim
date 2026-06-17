using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ColonyInitializer : MonoBehaviour
{
    [Header("Prefab del colono")]
    [SerializeField] private GameObject colonistPrefab;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text colonyNameText;

    [Header("Aparición inicial")]
    [SerializeField] private float spawnDistanceFromShelf = 1.5f;

    private void Start()
    {
        if (GameSetupData.Instance == null) return;

        string colonyName = GameSetupData.Instance.colonyName;

        if (string.IsNullOrWhiteSpace(colonyName))
        {
            colonyName = "Nueva Colonia";
        }

        colonyNameText.text = colonyName;

        if (InitialColonyPlacementManager.Instance != null)
        {
            InitialColonyPlacementManager.Instance.OnInitialShelfPlaced += SpawnInitialColonistsAroundShelf;
        }
    }

    private void OnDestroy()
    {
        if (InitialColonyPlacementManager.Instance != null)
        {
            InitialColonyPlacementManager.Instance.OnInitialShelfPlaced -= SpawnInitialColonistsAroundShelf;
        }
    }

    private void SpawnInitialColonistsAroundShelf(Vector3 shelfPosition)
    {
        for (int i = 0; i < GameSetupData.Instance.initialColonists.Count; i++)
        {
            ColonistSetupData data = GameSetupData.Instance.initialColonists[i];

            Vector3 spawnPosition = GetSpawnPositionAroundShelf(shelfPosition, i);

            GameObject colonistObject = Instantiate(colonistPrefab, spawnPosition, Quaternion.identity);

            ColonistIdentity identity = colonistObject.GetComponent<ColonistIdentity>();
            if (identity != null)
            {
                identity.SetName(data.colonistName);
            }

            ColonistVisuals visuals = colonistObject.GetComponent<ColonistVisuals>();
            if (visuals != null)
            {
                visuals.ApplySkin(data.skin);
            }
        }

        if (GameEndManager.Instance != null)
        {
            GameEndManager.Instance.SetColonyStarted();
        }
    }

    private Vector3 GetSpawnPositionAroundShelf(Vector3 shelfPosition, int index)
    {
        Vector3[] offsets =
        {
            new Vector3(-spawnDistanceFromShelf, -spawnDistanceFromShelf, 0f),
            new Vector3(0f, -spawnDistanceFromShelf, 0f),
            new Vector3(spawnDistanceFromShelf, -spawnDistanceFromShelf, 0f)
        };

        if (index < offsets.Length)
        {
            return shelfPosition + offsets[index];
        }

        return shelfPosition + new Vector3(index * 0.8f, -spawnDistanceFromShelf, 0f);
    }
}
