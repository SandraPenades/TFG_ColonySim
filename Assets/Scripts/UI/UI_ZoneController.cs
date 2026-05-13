using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ZoneSelector : MonoBehaviour
{
    public MouseController mouseController; 

    // Funciones para el OnClick() de cada botón
    public void SelectLoggingTool()
    {
        mouseController.SetMode(ZoneManager.ZoneType.Logging);
        mouseController.enabled = true;
    }

    public void SelectMiningTool()
    {
        mouseController.SetMode(ZoneManager.ZoneType.Mining);
        mouseController.enabled = true;
    }

    public void SelectHarvestingTool()
    {
        mouseController.SetMode(ZoneManager.ZoneType.Harvesting);
        mouseController.enabled = true;
    }
}