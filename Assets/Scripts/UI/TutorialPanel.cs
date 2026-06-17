using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPanel : MonoBehaviour
{
    public static TutorialPanel Instance;

    [System.Serializable]
    public class TutorialPage
    {
        public string title;

        [TextArea(5, 12)]
        public string body;

        public Sprite image;
    }

    [Header("Referencias del panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image tutorialImage;

    [Header("Botones de navegación")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Páginas del tutorial")]
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();

    private int currentPageIndex = 0;

    private float previousTimeScale = 1f;
    private bool tutorialIsOpen = false;
    private bool openedFromPauseMenu = false;

    [Header("Integración con pausa")]
    [SerializeField] private PauseMenuManager pauseMenuManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTutorial);
        }
    }

    private void Start()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void ShowTutorial()
    {
        ShowTutorial(false);
    }

    public void ShowTutorialFromPauseMenu()
    {
        ShowTutorial(true);
    }

    private void ShowTutorial(bool fromPauseMenu)
    {
        if (pages == null || pages.Count == 0)
        {
            Debug.LogWarning("[TutorialPanel] No hay páginas de tutorial configuradas.");
            return;
        }

        openedFromPauseMenu = fromPauseMenu;

        if (!tutorialIsOpen)
        {
            previousTimeScale = Time.timeScale;
        }

        tutorialIsOpen = true;
        currentPageIndex = 0;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        Time.timeScale = 0f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isPaused = true;
            GameManager.Instance.currentSpeed = 0f;
        }

        UpdatePage();
    }

    private void NextPage()
    {
        if (currentPageIndex >= pages.Count - 1)
        {
            CloseTutorial();
            return;
        }

        currentPageIndex++;
        UpdatePage();
    }

    private void PreviousPage()
    {
        if (currentPageIndex <= 0) return;

        currentPageIndex--;
        UpdatePage();
    }

    private void CloseTutorial()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        tutorialIsOpen = false;

        if (openedFromPauseMenu)
        {
            Time.timeScale = 0f;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.isPaused = true;
                GameManager.Instance.currentSpeed = 0f;
            }

            if (pauseMenuManager != null)
            {
                pauseMenuManager.BackToPauseMenu();
            }

            openedFromPauseMenu = false;
            return;
        }

        if (previousTimeScale <= 0f)
        {
            previousTimeScale = 1f;
        }

        Time.timeScale = previousTimeScale;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isPaused = false;
            GameManager.Instance.currentSpeed = previousTimeScale;
        }

        openedFromPauseMenu = false;
    }

    private void UpdatePage()
    {
        TutorialPage page = pages[currentPageIndex];

        if (titleText != null)
        {
            titleText.text = ReplaceFields(page.title);
        }

        if (bodyText != null)
        {
            bodyText.text = ReplaceFields(page.body);
        }

        if (tutorialImage != null)
        {
            if (page.image != null)
            {
                tutorialImage.gameObject.SetActive(true);
                tutorialImage.sprite = page.image;
                tutorialImage.preserveAspect = true;
            }
            else
            {
                tutorialImage.gameObject.SetActive(false);
            }
        }

        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(currentPageIndex > 0);
        }

        if (nextButtonText != null)
        {
            nextButtonText.text = currentPageIndex >= pages.Count - 1 ? "Entendido" : "Siguiente";
        }
    }

    private string ReplaceFields(string text)
    {
        string colonyName = "tu colonia";

        if (GameSetupData.Instance != null && !string.IsNullOrWhiteSpace(GameSetupData.Instance.colonyName))
        {
            colonyName = GameSetupData.Instance.colonyName;
        }

        return text.Replace("{colonyName}", colonyName);
    }
}