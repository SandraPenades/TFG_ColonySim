using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject zoneMenuPanel;
    public GameObject buildMenuPanel;
    public GameObject actionMenuPanel;

    public SelectionManager selectionManager;
    public MouseController mouseController;

    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text berryText;
    [SerializeField] private TMP_Text colonistCountText;

    [SerializeField] private string woodID = "Madera";
    [SerializeField] private string stoneID = "Piedra";
    [SerializeField] private string berryID = "Baya";

    [SerializeField] private Transform messageContainer;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private int maxMessages = 5;

    [SerializeField] private Sprite visitorIcon;
    [SerializeField] private Sprite thiefIcon;
    [SerializeField] private Sprite stormIcon;
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite infoIcon;

    private List<GameObject> activeMessages = new List<GameObject>();

    [SerializeField] private float uiRefreshTime = 1f;
    private float uiRefreshTimer = 0f;

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

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateResourceUI;
        }

        UpdateResourceUI();
        UpdateColonistCountUI();
    }

    private void Update()
    {
        uiRefreshTimer += Time.deltaTime;

        if (uiRefreshTimer >= uiRefreshTime)
        {
            uiRefreshTimer = 0f;
            UpdateColonistCountUI();
        }
    }

    public void ToggleZoneMenu()
    {
        if (zoneMenuPanel == null) return;
        if (InitialColonyPlacementManager.Instance != null && InitialColonyPlacementManager.Instance.IsPlacingInitialShelf) return;

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
        if (InitialColonyPlacementManager.Instance != null && InitialColonyPlacementManager.Instance.IsPlacingInitialShelf) return;

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

    public bool IsAnyGameplayPanelOpen()
    {
        bool zoneOpen = zoneMenuPanel != null && zoneMenuPanel.activeSelf;
        bool buildOpen = buildMenuPanel != null && buildMenuPanel.activeSelf;
        bool actionOpen = actionMenuPanel != null && actionMenuPanel.activeSelf;

        return zoneOpen || buildOpen || actionOpen;
    }

    public void CloseOpenGameplayPanels()
    {
        if (zoneMenuPanel != null && zoneMenuPanel.activeSelf)
        {
            CloseZoneMenu();
        }

        if (buildMenuPanel != null && buildMenuPanel.activeSelf)
        {
            CloseBuildMenu();
        }

        if (actionMenuPanel != null && actionMenuPanel.activeSelf)
        {
            actionMenuPanel.SetActive(false);
        }
    }

    private void UpdateResourceUI()
    {
        if (ResourceManager.Instance == null) return;

        if (woodText != null)
        {
            woodText.text = ResourceManager.Instance.GetResourceAmount(woodID).ToString();
        }

        if (stoneText != null)
        {
            stoneText.text = ResourceManager.Instance.GetResourceAmount(stoneID).ToString();
        }

        if (berryText != null)
        {
            berryText.text = ResourceManager.Instance.GetResourceAmount(berryID).ToString();
        }
    }

    public int GetAliveColonyMemberCount()
    {
        int count = 0;

        ColonistRecruitment[] colonists = FindObjectsByType<ColonistRecruitment>(FindObjectsSortMode.None);

        foreach (ColonistRecruitment colonist in colonists)
        {
            if (colonist == null) continue;
            if (!colonist.IsColonyMember) continue;

            AgentNeeds needs = colonist.GetComponent<AgentNeeds>();

            if (needs != null && needs.IsDead) continue;

            count++;
        }

        return count;
    }

    private void UpdateColonistCountUI()
    {
        int count = GetAliveColonyMemberCount();

        if (colonistCountText != null)
        {
            colonistCountText.text = count.ToString();
        }
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged -= UpdateResourceUI;
        }
    }

    public void ShowInfoMessage(string message)
    {
        ShowEventMessage(message, infoIcon);
    }

    public void ShowWarningMessage(string message)
    {
        ShowEventMessage(message, warningIcon);
    }

    public void ShowEventMessage(string message, Sprite icon)
    {
        if (messageContainer == null || messagePrefab == null)
        {
            Debug.LogWarning("[UIManager] Falta asignar Message Container o Message Prefab.");
            return;
        }

        GameObject newMessage = Instantiate(messagePrefab, messageContainer);

        TMP_Text[] texts = newMessage.GetComponentsInChildren<TMP_Text>();
        Image[] images = newMessage.GetComponentsInChildren<Image>();

        if (texts.Length > 0)
        {
            texts[0].text = message;
        }

        if (texts.Length > 1)
        {
            texts[1].text = GetMessageTimeText();
        }

        if (images.Length > 0 && icon != null)
        {
            images[0].sprite = icon;
        }

        activeMessages.Add(newMessage);

        while (activeMessages.Count > maxMessages)
        {
            GameObject oldestMessage = activeMessages[0];
            activeMessages.RemoveAt(0);

            if (oldestMessage != null)
            {
                Destroy(oldestMessage);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMessage();
        }
    }

    private string GetMessageTimeText()
    {
        if (GameManager.Instance != null)
        {
            return "Día " + GameManager.Instance.CurrentDay;
        }

        return "Día -";
    }

    public void ShowColonistDeathMessage(string colonistName)
    {
        ShowEventMessage(colonistName + " ha muerto.", warningIcon);
    }

    public void ShowVisitorMessage(string visitorName)
    {
        ShowEventMessage("Ha llegado un visitante: " + visitorName + ".", visitorIcon);
    }

    public void ShowColonistJoinedMessage(string colonistName)
    {
        ShowEventMessage(colonistName + " se ha unido a la colonia.", visitorIcon);
    }

    public void ShowThiefMessage()
    {
        ShowEventMessage("Ha llegado un visitante sospechoso.", thiefIcon);
    }

    public void ShowStolenResourceMessage(string itemName, int amount)
    {
        ShowEventMessage("El ladrón ha robado " + itemName + " x" + amount + ".", thiefIcon);
    }

    public void ShowThiefLeftWithoutStealingMessage()
    {
        ShowEventMessage("El ladrón se ha marchado sin robar.", thiefIcon);
    }

    public void ShowStormMessage()
    {
        ShowEventMessage("Se acerca una tormenta.", stormIcon);
    }

    public void ShowEndStormMessage()
    {
        ShowEventMessage("La tormenta ha terminado.", stormIcon);
    }
}
