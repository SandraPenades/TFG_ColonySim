using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    [Header("Referencias de zona")]
    public ZoneManager zoneManager;
    public Grid mainGrid;

    [Header("Visualización de selección")]
    public GameObject selectionBoxVisual;

    private SpriteRenderer selectionRenderer; // Para cambiar el color

    private Vector3 startMousePos;
    private bool isDragging = false;
    private bool isErasing = false;

    [Header("Modo de zona activo")]
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

        // Clic izquierdo: crear zona
        if (Input.GetMouseButtonDown(0))
        {
            StartAreaSelection(false);
        }

        // Clic derecho: borrar zona
        if (Input.GetMouseButtonDown(1))
        {
            StartAreaSelection(true);
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePos.z = 0;
            UpdateSelectionBoxVisual(currentMousePos);
        }

        // Soltar clic izquierdo
        if (Input.GetMouseButtonUp(0) && isDragging && !isErasing)
        {
            FinishAreaSelection();
        }

        // Soltar clic derecho
        if (Input.GetMouseButtonUp(1) && isDragging && isErasing)
        {
            FinishAreaSelection();
        }
    }

    private void StartAreaSelection(bool eraseMode)
    {
        if (currentMode == ZoneManager.ZoneType.None) return;

        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startMousePos.z = 0;

        isDragging = true;
        isErasing = eraseMode;

        if (selectionBoxVisual != null)
        {
            selectionBoxVisual.SetActive(true);
        }

        UpdateSelectionColor();
        UpdateSelectionBoxVisual(startMousePos);
    }

    private void FinishAreaSelection()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        Vector3 endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endMousePos.z = 0;

        isDragging = false;

        if (selectionBoxVisual != null)
        {
            selectionBoxVisual.SetActive(false);
        }

        ApplyZone(startMousePos, endMousePos, isErasing);

        isErasing = false;
    }

    void UpdateSelectionColor()
    {
        if (selectionRenderer == null) return;

        Color color;

        switch (currentMode)
        {
            case ZoneManager.ZoneType.Logging: 
                color = new Color(0.28f, 0.60f, 0.34f, 0.8f); 
                break;
            case ZoneManager.ZoneType.Mining: 
                color = new Color(0.50f, 0.55f, 0.70f, 0.8f); 
                break;
            case ZoneManager.ZoneType.Harvesting: 
                color = new Color(0.90f, 0.27f, 0.30f, 0.8f); 
                break;
            default: 
                color = new Color(0.5f, 0.5f, 0.5f, 0.8f); 
                break;
        }

        if (isErasing)
        {
            color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }

        selectionRenderer.color = color;
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

        if (zoneManager != null)
        {
            if (currentMode == ZoneManager.ZoneType.None)
            {
                zoneManager.HideAllZones();
            }
            else
            {
                zoneManager.ShowOnlyZone(currentMode);
            }
        }
    }

    void ApplyZone(Vector3 startWorld, Vector3 endWorld, bool erase)
    {
        Vector3Int startCell = mainGrid.WorldToCell(startWorld);
        Vector3Int endCell = mainGrid.WorldToCell(endWorld);

        int minX = Mathf.Min(startCell.x, endCell.x);
        int maxX = Mathf.Max(startCell.x, endCell.x);
        int minY = Mathf.Min(startCell.y, endCell.y);
        int maxY = Mathf.Max(startCell.y, endCell.y);

        BoundsInt area = new BoundsInt(new Vector3Int(minX, minY, 0), new Vector3Int(maxX - minX + 1, maxY - minY + 1, 1));
        
        if (erase)
        {
            zoneManager.EraseZone(area, currentMode);
        }
        else
        {
            zoneManager.MarkZone(area, currentMode);
        }
    }

    // Cuando se apaga el script, el cuadro desaparece para que no se quede congelado
    void OnDisable()
    {
        isDragging = false;
        isErasing = false;

        if (selectionBoxVisual != null)
        {
            selectionBoxVisual.SetActive(false);
        }
    }
}
