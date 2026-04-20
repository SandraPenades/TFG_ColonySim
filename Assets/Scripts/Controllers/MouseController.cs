using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    public ZoneManager zoneManager;
    public Grid mainGrid;
    public GameObject selectionBoxVisual;
    private SpriteRenderer selectionRenderer; // Para cambiar el color

    private Vector3 startMousePos;
    private bool isDragging = false;

    public ZoneManager.ZoneType currentMode = ZoneManager.ZoneType.Logging;

    void Awake()
    {
        if (selectionBoxVisual != null)
        {
            selectionRenderer = selectionBoxVisual.GetComponent<SpriteRenderer>();
        }
    }

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

            UpdateSelectionColor();
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

    void UpdateSelectionColor()
    {
        if (selectionRenderer == null) return;

        switch (currentMode)
        {
            case ZoneManager.ZoneType.Logging: selectionRenderer.color = new Color(1f, 0.92f, 0.016f, 0.4f); break;
            case ZoneManager.ZoneType.Mining: selectionRenderer.color = new Color(0f, 1f, 1f, 0.4f); break;
            case ZoneManager.ZoneType.Harvesting: selectionRenderer.color = new Color(1f, 0f, 1f, 0.4f); break;
            case ZoneManager.ZoneType.None: selectionRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.4f); break;
        }
    }

    void UpdateSelectionBoxVisual(Vector3 currentMousePos)
    {
        // Convertir las posiciones del mundo a coordenadas de celda (Int)
        Vector3Int startCell = mainGrid.WorldToCell(startMousePos);
        Vector3Int currentCell = mainGrid.WorldToCell(currentMousePos);

        // Calcular los límites de la selección en la rejilla
        int minX = Mathf.Min(startCell.x, currentCell.x);
        int maxX = Mathf.Max(startCell.x, currentCell.x);
        int minY = Mathf.Min(startCell.y, currentCell.y);
        int maxY = Mathf.Max(startCell.y, currentCell.y);

        // Calcular la posición del mundo de esas celdas (esquinas)
        Vector3 minWorld = mainGrid.CellToWorld(new Vector3Int(minX, minY, 0));
        Vector3 maxWorld = mainGrid.CellToWorld(new Vector3Int(maxX + 1, maxY + 1, 0));

        // Ajustar el visualizador para que encaje perfectamente en la cuadrícula
        selectionBoxVisual.transform.position = (minWorld + maxWorld) / 2f;
        selectionBoxVisual.transform.localScale = new Vector3(maxWorld.x - minWorld.x, maxWorld.y - minWorld.y, 1);
    }

    public void SetMode(ZoneManager.ZoneType newMode)
    {
        currentMode = newMode;
        UpdateSelectionColor(); // Actualizar color al cambiar herramienta
    }

    void ApplyZone(Vector3 startWorld, Vector3 endWorld)
    {
        Vector3Int startCell = mainGrid.WorldToCell(startWorld);
        Vector3Int endCell = mainGrid.WorldToCell(endWorld);

        int minX = Mathf.Min(startCell.x, endCell.x);
        int maxX = Mathf.Max(startCell.x, endCell.x);
        int minY = Mathf.Min(startCell.y, endCell.y);
        int maxY = Mathf.Max(startCell.y, endCell.y);

        BoundsInt area = new BoundsInt(new Vector3Int(minX, minY, 0), new Vector3Int(maxX - minX + 1, maxY - minY + 1, 1));
        zoneManager.MarkZone(area, currentMode);
    }

    // Cuando se apaga el script, el cuadro desaparece para que no se quede congelado
    void OnDisable()
    {
        isDragging = false;
        if (selectionBoxVisual != null)
        {
            selectionBoxVisual.SetActive(false);
        }
    }
}
