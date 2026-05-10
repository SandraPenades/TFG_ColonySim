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

    private GameObject selectedColonist;
    private AgentMovement selectedColonistMovement;
    private ColonistRecruitment selectedColonistRecruitment;

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
        }

        selectedColonist = colono;
        selectedColonistMovement = colono.GetComponent<AgentMovement>();
        selectedColonistRecruitment = colono.GetComponent<ColonistRecruitment>();

        isColonistSelected = true;
        actionMenuPanel.SetActive(true);

        UpdateRecruitButtonText();
    }

    public void DeselectAll()
    {
        selectedColonist = null;
        selectedColonistMovement = null;
        selectedColonistRecruitment = null;
        
        isColonistSelected = false;
        actionMenuPanel.SetActive(false);
    }

    void OnActionButtonClicked()
    {
        if (!isColonistSelected) return;
        if (selectedColonistRecruitment == null) return;

        selectedColonistRecruitment.ToggleRecruitment();

        UpdateRecruitButtonText();
    }

    void UpdateRecruitButtonText()
    {
        if (actionText == null) return;

        if (!isColonistSelected || selectedColonistRecruitment == null)
        {
            actionText.text = "Reclutar";
            return;
        }

        actionText.text = selectedColonistRecruitment.IsRecruited ? "Licenciar" : "Reclutar";
    }
}