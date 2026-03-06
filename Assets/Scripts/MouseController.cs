using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    public ZoneManager zoneManager;
    public Grid mainGrid;
    public GameObject selectionBoxVisual;

    private Vector3 startMousePos;
    private bool isDragging = false;

    public ZoneManager.ZoneType currentMode = ZoneManager.ZoneType.Logging;

    void Update()
    {
        // Si se toca la UI, no se modifica el mapa
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Cuando se hace clic, se guarda el punto de inicio
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startMousePos.z = 0;
            isDragging = true;

            // Activamos el visualizador y lo ponemos en el punto inicial
            selectionBoxVisual.SetActive(true);
            UpdateSelectionBoxVisual(startMousePos);
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePos.z = 0;
            UpdateSelectionBoxVisual(currentMousePos);
        }

        // Al soltar el clic, se calcula el recuadro final del área
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector3 endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            endMousePos.z = 0;
            isDragging = false;

            selectionBoxVisual.SetActive(false);

            ApplyZone(startMousePos, endMousePos);
        }
    }

    void UpdateSelectionBoxVisual(Vector3 currentMousePos)
    {
        // 1. Calcular el punto central entre el inicio y el ratón actual
        Vector3 center = (startMousePos + currentMousePos) / 2f;

        // 2. Calcular el tamaño (ancho y alto) usando valor absoluto
        float sizeX = Mathf.Abs(startMousePos.x - currentMousePos.x);
        float sizeY = Mathf.Abs(startMousePos.y - currentMousePos.y);

        // 3. Aplicar posición y escala
        selectionBoxVisual.transform.position = center;
        selectionBoxVisual.transform.localScale = new Vector3(sizeX, sizeY, 1);
    }

    void ApplyZone(Vector3 startWorld, Vector3 endWorld)
    {
        Vector3Int startCell = mainGrid.WorldToCell(startWorld);
        Vector3Int endCell = mainGrid.WorldToCell(endWorld);

        int minX = Mathf.Min(startCell.x, endCell.x);
        int maxX = Mathf.Max(startCell.x, endCell.x);
        int minY = Mathf.Min(startCell.y, endCell.y);
        int maxY = Mathf.Max(startCell.y, endCell.y);

        BoundsInt area = new BoundsInt();
        area.xMin = minX;
        area.xMax = maxX + 1;
        area.yMin = minY;
        area.yMax = maxY + 1;
        area.zMin = 0;
        area.zMax = 1;

        zoneManager.MarkZone(area, currentMode);
    }
}
