using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado de la partida")]
    public bool isPaused = false;
    public float currentSpeed = 1f;

    [Header("Velocidades disponibles")]
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;

    private float lastSpeedBeforePause = 1f;

    [Header("Sistema de días")]
    [SerializeField] private int currentDay = 0;
    [SerializeField] private float secondsPerDay = 60f;

    [Header("Interfaz de tiempo")]
    [SerializeField] private TMP_Text dayText;

    private float dayTimer = 0f;

    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetNormalSpeed();
        UpdateDayText();
    }

    private void Update()
    {
        UpdateDayCounter();
    }

    private void UpdateDayCounter()
    {
        if (isPaused) return;

        dayTimer += Time.deltaTime;

        if (dayTimer >= secondsPerDay)
        {
            dayTimer -= secondsPerDay;
            currentDay++;

            UpdateDayText();
        }
    }

    private void UpdateDayText()
    {
        if (dayText != null)
        {
            dayText.text = "Día " + currentDay;
        }
    }

    public void PauseGame()
    {
        if (EventManager.Instance != null && EventManager.Instance.IsThiefEventActive())
        {
            Debug.Log("No puedes pausar mientras hay un ladrón en la colonia.");
            return;
        }

        if (isPaused) return;

        lastSpeedBeforePause = currentSpeed;
        isPaused = true;
        currentSpeed = 0f;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (lastSpeedBeforePause <= 0f)
        {
            lastSpeedBeforePause = normalSpeed;
        }

        currentSpeed = lastSpeedBeforePause;
        Time.timeScale = currentSpeed;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void SetNormalSpeed()
    {
        isPaused = false;
        currentSpeed = normalSpeed;
        lastSpeedBeforePause = normalSpeed;
        Time.timeScale = currentSpeed;
    }

    public void SetFastSpeed()
    {
        isPaused = false;
        currentSpeed = fastSpeed;
        lastSpeedBeforePause = fastSpeed;
        Time.timeScale = currentSpeed;
    }

    public void ForcePauseGame()
    {
        lastSpeedBeforePause = currentSpeed;
        isPaused = true;
        currentSpeed = 0f;
        Time.timeScale = 0f;
    }
}