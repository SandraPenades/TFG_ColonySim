using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject zoneMenuPanel;
    public GameObject buildMenuPanel;

    public SelectionManager selectionManager;
    public MouseController mouseController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (zoneMenuPanel != null) zoneMenuPanel.SetActive(false);
        if (buildMenuPanel != null) buildMenuPanel.SetActive(false);

        if (mouseController != null) mouseController.enabled = false;

        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.HideAllZones();
        }

        if (BuilderManager.Instance != null)
        {
            BuilderManager.Instance.CancelBuildMode();
        }
    }

    public void ToggleZoneMenu()
    {
        if (zoneMenuPanel == null) return;

        bool isCurrentlyActive = zoneMenuPanel.activeSelf;
        bool newState = !isCurrentlyActive;

        if (newState)
        {
            // Si abrimos zonas, cerramos construcción
            CloseBuildMenu();

            if (selectionManager != null)
            {
                selectionManager.DeselectAll();
            }
        }

        zoneMenuPanel.SetActive(newState);

        if (mouseController != null)
        {
            mouseController.enabled = newState;
        }

        if (!newState && ZoneManager.Instance != null)
        {
            ZoneManager.Instance.HideAllZones();
        }
    }

    public void CloseZoneMenu()
    {
        if (zoneMenuPanel != null)
        {
            zoneMenuPanel.SetActive(false);
        }

        if (mouseController != null)
        {
            mouseController.enabled = false;
        }

        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.HideAllZones();
        }
    }

    public void ToggleBuildMenu()
    {
        if (buildMenuPanel == null) return;

        bool isCurrentlyActive = buildMenuPanel.activeSelf;
        bool newState = !isCurrentlyActive;

        if (newState)
        {
            // Si abrimos construcción, cerramos zonas
            CloseZoneMenu();

            if (selectionManager != null)
            {
                selectionManager.DeselectAll();
            }
        }

        buildMenuPanel.SetActive(newState);

        if (!newState && BuilderManager.Instance != null)
        {
            BuilderManager.Instance.CancelBuildMode();
        }
    }

    public void CloseBuildMenu()
    {
        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(false);
        }

        if (BuilderManager.Instance != null)
        {
            BuilderManager.Instance.CancelBuildMode();
        }
    }
}
