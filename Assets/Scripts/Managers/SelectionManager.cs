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
    private AgentMovement selectedColonistMovement;
    private AgentBrain selectedColonistBrain;

    private bool isColonistSelected = false;
    private bool isRecruited = false;

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

        // Si hacemos clic en el vacío (no en un objeto o colono)
        DeselectAll();
    }

    void HandleRightClick()
    {
        // Si el colono está reclutado y está seleccionado y hacemos clic derecho, se mueve
        if (isColonistSelected && isRecruited && selectedColonistMovement != null)
        {
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = 0;

            selectedColonistMovement.MoveTo(targetPos);
            Debug.Log("[SelectionManager] Colono moviéndose en modo recluta a: " + targetPos);
        }
    }

    void SelectColonist(GameObject colono)
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseZoneMenu();
        selectedColonistMovement = colono.GetComponent<AgentMovement>();
        selectedColonistBrain = colono.GetComponent<AgentBrain>();

        isColonistSelected = true;
        actionMenuPanel.SetActive(true);

        UpdateRecruitButtonText();
    }

    public void DeselectAll()
    {
        isColonistSelected = false;
        selectedColonistMovement = null;
        selectedColonistBrain = null;
        actionMenuPanel.SetActive(false);
    }

    void OnActionButtonClicked()
    {
        if (isColonistSelected && selectedColonistBrain != null)
        {
            isRecruited = !isRecruited;

            if (isRecruited)
            {
                // "Apagar" la IA
                selectedColonistBrain.AbortCurrentAction();
                selectedColonistBrain.enabled = false;
                selectedColonistMovement.StopMoving();
            }
            else
            {
                // "Encender" la IA
                selectedColonistBrain.enabled = true;
            }

            UpdateRecruitButtonText();
        }
    }

    void UpdateRecruitButtonText()
    {
        if (isRecruited)
        {
            actionText.text = "Licenciar";
        }
        else
        {
            actionText.text = "Reclutar";
        }
    }
}