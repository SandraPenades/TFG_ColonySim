using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [SerializeField] private float secondsPerDay = 60f;
    [SerializeField] private int targetDay = 100;

    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text objectiveText;

    public int CurrentDay { get; private set; } = 1;
    public bool HasWon { get; private set; } = false;

    private float dayTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (HasWon) return;

        dayTimer += Time.deltaTime;

        if (dayTimer >= secondsPerDay)
        {
            dayTimer = 0f;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        CurrentDay++;

        UpdateUI();

        if (CurrentDay >= targetDay)
        {
            TriggerVictory();
        }
    }

    private void UpdateUI()
    {
        if (dayText != null)
        {
            dayText.text = "Día " + CurrentDay;
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objetivo: sobrevivir " + targetDay + " días";
        }
    }

    private void TriggerVictory()
    {
        HasWon = true;

        Debug.Log("¡Victoria! La colonia ha sobrevivido " + targetDay + " días.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }

        // Aquí podemos abrir un panel de victoria.
    }
}
