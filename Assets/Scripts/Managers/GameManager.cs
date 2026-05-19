using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isPaused = false;
    public float currentSpeed = 1f;

    public float normalSpeed = 1f;
    public float fastSpeed = 2f;
    public float veryFastSpeed = 3f;

    private float lastSpeedBeforePause = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetNormalSpeed();
    }

    public void PauseGame()
    {
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

    public void SetVeryFastSpeed()
    {
        isPaused = false;
        currentSpeed = veryFastSpeed;
        lastSpeedBeforePause = veryFastSpeed;
        Time.timeScale = currentSpeed;
    }
}
