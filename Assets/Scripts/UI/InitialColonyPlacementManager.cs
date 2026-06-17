using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialColonyPlacementManager : MonoBehaviour
{
    public static InitialColonyPlacementManager Instance;

    [Header("Estantería inicial")]
    [SerializeField] private GameObject initialShelfBlueprintPrefab;

    public bool IsPlacingInitialShelf { get; private set; } = false;

    private bool hasPlacedShelf = false;

    public System.Action<Vector3> OnInitialShelfPlaced;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetPlacementEnabled(false);
    }

    public void StartInitialShelfPlacement()
    {
        if (hasPlacedShelf) return;

        IsPlacingInitialShelf = true;

        if (BuilderManager.Instance != null)
        {
            BuilderManager.Instance.StartInitialShelfPlacement(
                initialShelfBlueprintPrefab,
                FinishInitialShelfPlacement
            );
        }
        else
        {
            Debug.LogWarning("[InitialColonyPlacementManager] No existe BuilderManager.");
        }
    }

    public void SetPlacementEnabled(bool enabled)
    {
        IsPlacingInitialShelf = enabled;
    }

    private void FinishInitialShelfPlacement(Vector3 shelfPosition)
    {
        hasPlacedShelf = true;
        IsPlacingInitialShelf = false;

        OnInitialShelfPlaced?.Invoke(shelfPosition);

        if (TutorialPanel.Instance != null)
        {
            TutorialPanel.Instance.ShowTutorial();
        }
    }
}