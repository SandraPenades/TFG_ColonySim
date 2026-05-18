using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public GameObject actionMenuPanel;
    public Button actionButton;
    public TextMeshProUGUI actionText;
    public ColonistInfoPanel colonistInfoPanel;
    public WorkPriorityPanel workPriorityPanel;

    private GameObject selectedColonist;
    private AgentMovement selectedColonistMovement;
    private ColonistRecruitment selectedColonistRecruitment;
    private ConstructedBuilding selectedBuilding;

    private bool isColonistSelected = false;

    void Start()
    {
        actionMenuPanel.SetActive(false);
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();
    }

    void HandleLeftClick()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // Si hacemos clic en un colono
        if (hit.collider != null && hit.collider.CompareTag("Colono"))
        {
            SelectColonist(hit.collider.gameObject);
            return;
        }

        // Si hacemos clic en una construcción
        if (hit.collider != null)
        {
            ConstructedBuilding building = hit.collider.GetComponent<ConstructedBuilding>();

            if (building == null)
            {
                building = hit.collider.GetComponentInParent<ConstructedBuilding>();
            }

            if (building != null)
            {
                SelectBuilding(building);
                return;
            }
        }

        // Si hacemos clic en el vacío
        DeselectAll();
    }

    void HandleRightClick()
    {
        if (!isColonistSelected) return;
        if (selectedColonistMovement == null) return;
        if (selectedColonistRecruitment == null) return;

        // Solo se permite mover manualmente si ese colono concreto está reclutado
        if (selectedColonistRecruitment.IsRecruited)
        {
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = 0;

            selectedColonistMovement.MoveTo(targetPos);
            Debug.Log("[SelectionManager] Colono moviéndose en modo recluta a: " + targetPos);
        }
    }

    void SelectColonist(GameObject colono)
    {
        if (UIManager.Instance != null) 
        {
            UIManager.Instance.CloseZoneMenu();
            UIManager.Instance.CloseBuildMenu();
        }

        selectedBuilding = null;

        selectedColonist = colono;
        selectedColonistMovement = colono.GetComponent<AgentMovement>();
        selectedColonistRecruitment = colono.GetComponent<ColonistRecruitment>();

        isColonistSelected = true;
        actionMenuPanel.SetActive(true);

        if (colonistInfoPanel != null)
        {
            colonistInfoPanel.SetColonist(colono);
        }
        if (workPriorityPanel != null)
        {
            workPriorityPanel.SetColonist(colono);
        }

        UpdateActionButtonText();
    }

    void SelectBuilding(ConstructedBuilding building)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseZoneMenu();
            UIManager.Instance.CloseBuildMenu();
        }

        selectedColonist = null;
        selectedColonistMovement = null;
        selectedColonistRecruitment = null;

        selectedBuilding = building;

        isColonistSelected = false;
        actionMenuPanel.SetActive(true);

        UpdateActionButtonText();
    }

    public void DeselectAll()
    {
        selectedColonist = null;
        selectedColonistMovement = null;
        selectedColonistRecruitment = null;
        selectedBuilding = null;
        
        isColonistSelected = false;
        actionMenuPanel.SetActive(false);

        if (actionMenuPanel != null)
        {
            actionMenuPanel.SetActive(false);
        }

        if (colonistInfoPanel != null)
        {
            colonistInfoPanel.ClearPanel();
        }

        if (workPriorityPanel != null)
        {
            workPriorityPanel.ClearPanel();
        }
    }

    void OnActionButtonClicked()
    {
        if (selectedColonistRecruitment != null)
        {
            selectedColonistRecruitment.ToggleRecruitment();
            UpdateActionButtonText();
            return;
        } 
        if (selectedBuilding != null)
        {
            ToggleBuildDeconstruction();
            UpdateActionButtonText();
            return;
        } 
    }

    void UpdateActionButtonText()
    {
        if (actionText == null) return;

        if (selectedColonistRecruitment != null)
        {
            actionText.text = selectedColonistRecruitment.IsRecruited ? "Licenciar" : "Reclutar";
            return;
        }

        if (selectedBuilding != null)
        {
            actionText.text = selectedBuilding.isMarkedForDeconstruction ? "Cancelar" : "Deconstruir";
            return;
        }

        actionText.text = "";
    }

    private void ToggleBuildDeconstruction()
    {
        if (selectedBuilding == null) return;
        if (ConstructionManager.Instance == null) return;

        if (selectedBuilding.isMarkedForDeconstruction)
        {
            ConstructionManager.Instance.UnmarkForDeconstruction(selectedBuilding);
        }
        else
        {
            ConstructionManager.Instance.MarkForDeconstruction(selectedBuilding);
        }
    }
}