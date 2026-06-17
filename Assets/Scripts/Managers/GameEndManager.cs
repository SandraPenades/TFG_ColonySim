using System.Collections;
using UnityEngine;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance;

    [Header("Condiciones de finalización")]
    [SerializeField] private int victoryDay = 100;
    [SerializeField] private float checkInterval = 1f;

    [Header("Panel de victoria")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winTimeText;

    [Header("Panel de derrota")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text loseTimeText;

    private bool gameEnded = false;
    private bool colonyStarted = false;

    private float startTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        startTime = Time.time;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        StartCoroutine(CheckEndConditionsRoutine());
    }

    private IEnumerator CheckEndConditionsRoutine()
    {
        while (!gameEnded)
        {
            CheckVictoryCondition();
            CheckLoseCondition();

            yield return new WaitForSeconds(checkInterval);
        }
    }

    public void SetColonyStarted()
    {
        colonyStarted = true;
        startTime = Time.time;
    }

    private void CheckVictoryCondition()
    {
        if (gameEnded) return;
        if (!colonyStarted) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentDay >= victoryDay)
        {
            WinGame();
        }
    }

    private void CheckLoseCondition()
    {
        if (gameEnded) return;
        if (!colonyStarted) return;
        if (UIManager.Instance == null) return;

        int aliveColonists = UIManager.Instance.GetAliveColonyMemberCount();

        if (aliveColonists <= 0)
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winTimeText != null)
        {
            winTimeText.text = GetFormattedElapsedTime();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForcePauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    private void LoseGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        if (loseTimeText != null)
        {
            loseTimeText.text = GetFormattedElapsedTime();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartDefeatRain();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLose();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForcePauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    private string GetFormattedElapsedTime()
    {
        float elapsed = Time.time - startTime;

        int hours = Mathf.FloorToInt(elapsed / 3600f);
        int minutes = Mathf.FloorToInt((elapsed % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);

        if (hours > 0)
        {
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        return $"{minutes:00}:{seconds:00}";
    }

    public bool HasGameEnded()
    {
        return gameEnded;
    }
}