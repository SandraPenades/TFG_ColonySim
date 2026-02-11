using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public GameObject actionMenuPanel;
    public Button actionButton;
    public TextMeshProUGUI actionText;

    public Tilemap obstaclesTilemap;

    private MovementController selectedColonist;
    private Vector3Int selectedTilePos;
    private bool isColonistSelected = false;

    void Start()
    {
        // Al empezar, escondemos el menú y configuramos el botón
        actionMenuPanel.SetActive(false);
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick(); // He corregido el nombre a Handle (Manejar)
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        
        // CORRECCIÓN 1: Quitamos el punto y coma traicionero del final del if
        // CORRECCIÓN 2: Añadimos () al GetComponent
        if (hit.collider != null && hit.collider.CompareTag("Colono"))
        {
            SelectColonist(hit.collider.GetComponent<MovementController>());
            return;
        }

        // Si seleccionamos un objeto interactuable
        Vector3Int cellPos = obstaclesTilemap.WorldToCell(mousePos);
        
        // CORRECCIÓN 3: HasTile (no HasTitle)
        if (obstaclesTilemap.HasTile(cellPos))
        {
            SelectTree(cellPos);
            return;
        }

        // Si no hay nada para interactuar
        DeselectAll();
    }

    void HandleRightClick()
    {
        // NOTA: Asegúrate de que en MovementController la variable se llame "isRecruited"
        if (isColonistSelected && selectedColonist != null && selectedColonist.isRecruited)
        {
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = 0;
            selectedColonist.MoveToPosition(targetPos); // Asegúrate de que este método existe en MovementController
            Debug.Log("Colono moviéndose a: " + targetPos);
        }
    }

    void SelectColonist(MovementController colono)
    {
        selectedColonist = colono;
        isColonistSelected = true;

        actionMenuPanel.SetActive(true);

        // Cambiar texto del botón según estado
        if (colono.isRecruited)
            actionText.text = "Licenciar (F)";
        else
            actionText.text = "Reclutar (R)";
    }

    void SelectTree(Vector3Int tilePos)
    {
        selectedTilePos = tilePos;
        isColonistSelected = false;
        selectedColonist = null;

        actionMenuPanel.SetActive(true);
        
        // CORRECCIÓN 4: Accedemos a la propiedad .text
        actionText.text = "Talar";
    }

    void DeselectAll()
    {
        actionMenuPanel.SetActive(false);
    }

    void OnActionButtonClicked()
    {
        if (isColonistSelected && selectedColonist != null)
        {
            selectedColonist.isRecruited = !selectedColonist.isRecruited;

            if (selectedColonist.isRecruited)
            {
                actionText.text = "Licenciar";
            }
            else
            {
                actionText.text = "Reclutar";
            }

            Debug.Log("Estado colono cambiado a: " + selectedColonist.isRecruited);
        }
        else
        {
            MovementController worker = FindFirstObjectByType<MovementController>();

            if (worker != null)
            {
                Debug.Log("Enviando al colono a talar en: " + selectedTilePos);

                worker.GoAndChop(selectedTilePos, obstaclesTilemap);
            }
            else
            {
                Debug.Log("No hay colonos disponibles");
            }

            actionMenuPanel.SetActive(false);
        }
    }
}