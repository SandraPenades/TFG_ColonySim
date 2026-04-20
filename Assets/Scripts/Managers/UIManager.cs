using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject zoneMenuPanel;

    public SelectionManager selectionManager;
    public MouseController mouseController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Que el panel empiece oculto
        if (zoneMenuPanel != null) zoneMenuPanel.SetActive(false);
        if (mouseController != null) mouseController.enabled = false;
    }

    public void ToggleZoneMenu()
    {
        bool isCurrentlyActive = zoneMenuPanel.activeSelf;

        if (!isCurrentlyActive)
        {
            // Primero deseleccionar todo
            if (selectionManager != null)
            {
                selectionManager.DeselectAll();
            }
        }

        // Si está apagado se enciende y viceversa
        bool newState = !isCurrentlyActive;
        zoneMenuPanel.SetActive(newState);

        // Activar/Desactivar la lógica del ratón para zonear
        if (!newState && mouseController != null)
        {
            mouseController.enabled = false;
        }
    }

    public void CloseZoneMenu()
    {
        if (zoneMenuPanel != null) zoneMenuPanel.SetActive(false);
        if (mouseController != null) mouseController.enabled = false;
    }
}
