using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // Evento visitante
    [Header("Visitantes y ladrones")]
    public GameObject visitorColonistPrefab;
    public Transform visitorSpawnPoint;
    public Transform visitorExitPoint;

    public float firstVisitorDelay = 60f;
    public bool spawnVisitorOnStart = true;

    [Header("Configuración de ladrones")]
    [SerializeField] private float thiefChance = 0.90f;

    private bool visitorEventTriggered = false;
    private bool thiefEventActive = false;

    private List<string> Names = new List<string> 
    {
        "Sandra", "Adrian", "Patricia", "Jorge", 
        "Lucia", "Oreto", "Ana", "Carolina", "Ari", 
        "Nai", "Eira", "Lior", "Maren", "Zuri", "King",
        "Elur", "Rune", "Soren", "Talen", "Vega", "Bee",
        "Lumi", "Riel", "Nara", "Levi", "Anya", "Jaime",
        "Sam", "Kiri", "Happy", "Stark", "Lucky", "Leaf"
    };

    [Header("Apariencias de visitantes")]
    [SerializeField] private CharacterSkinData[] availableVisitorSkins;

    private Coroutine visitorCoroutine;

    // Evento tormenta
    [Header("Tormenta: efectos visuales")]
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private Image stormOverlay;
    
    [Header("Tormenta: aparición automática")]
    [SerializeField] private bool automaticStorms = true;

    [SerializeField] private float firstStormMinDelay = 120f;
    [SerializeField] private float firstStormMaxDelay = 240f;

    [SerializeField] private float stormMinDelay = 180f;
    [SerializeField] private float stormMaxDelay = 360f;

    private Coroutine stormScheduleCoroutine;

    [Header("Tormenta: duración y efecto")]
    [SerializeField] private float stormMinDuration = 30f;
    [SerializeField] private float stormMaxDuration = 150f;
    [SerializeField] private float overlayAlpha = 0.25f;
    private bool stormActive = false;

    [SerializeField] private float stormSpeedMultiplier = 0.5f;

    private Dictionary<AgentMovement, float> originalSpeeds = new Dictionary<AgentMovement, float>();

    private Coroutine stormCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StopStormVisuals();

        if (spawnVisitorOnStart)
        {
            visitorCoroutine = StartCoroutine(VisitorRoutine());
        }

        if (automaticStorms)
        {
            stormScheduleCoroutine = StartCoroutine(StormScheduleRoutine());
        }
    }

    private void Update()
    {
        // Esto es para provocar el evento durante el testeo
        /*
        if (Input.GetKeyDown(KeyCode.N))
        {
            TriggerNewColonistEvent();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            StartStorm();
        }
        */
    }

    // Evento Visitante

    private IEnumerator VisitorRoutine()
    {
        yield return new WaitForSeconds(firstVisitorDelay);

        TriggerNewColonistEvent();

        visitorCoroutine = null;
    }

    public void TriggerNewColonistEvent()
    {
        if (visitorEventTriggered) return;
        if (visitorColonistPrefab == null) return;
        if (visitorSpawnPoint == null) return;

        GameObject visitor = Instantiate(
            visitorColonistPrefab,
            visitorSpawnPoint.position,
            Quaternion.identity
        );

        int randomNum = Random.Range(0, Names.Count);
        string visitorName = Names[randomNum];

        visitor.name = visitorName;

        ColonistIdentity identity = visitor.GetComponent<ColonistIdentity>();

        if (identity != null)
        {
            identity.SetName(visitorName);
        }

        if (availableVisitorSkins != null && availableVisitorSkins.Length > 0)
        {
            int randomIndex = Random.Range(0, availableVisitorSkins.Length);

            ColonistVisuals visuals = visitor.GetComponent<ColonistVisuals>();

            if (visuals != null)
            {
                visuals.ApplySkin(availableVisitorSkins[randomIndex]);
            }
        }

        bool isThief = Random.value < thiefChance;

        VisitorColonist visitorScript = visitor.GetComponent<VisitorColonist>();
        ThiefVisitor thiefScript = visitor.GetComponent<ThiefVisitor>();

        if (isThief)
        {
            if (visitorScript != null)
            {
                visitorScript.enabled = false;
            }

            if (thiefScript != null)
            {
                thiefScript.enabled = true;
                thiefScript.exitPoint = visitorExitPoint;

                Door.RefreshAllDoorsFor(visitor);
                thiefScript.StartThiefEvent();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowThiefMessage();
            }
        }
        else
        {
            if (thiefScript != null)
            {
                thiefScript.enabled = false;
            }

            if (visitorScript != null)
            {
                visitorScript.exitPoint = visitorExitPoint;
                visitorScript.enabled = true;
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVisitorMessage(visitorName);
            }
        }

        visitorEventTriggered = true;
    }

    public void VisitorEventFinished()
    {
        visitorEventTriggered = false;

        if (spawnVisitorOnStart && visitorCoroutine == null)
        {
            visitorCoroutine = StartCoroutine(VisitorRoutine());
        }
    }

    public bool IsThiefEventActive()
    {
        return thiefEventActive;
    }

    public void SetThiefEventActive(bool value)
    {
        thiefEventActive = value;
    }

    // Evento Tormenta

    private IEnumerator StormScheduleRoutine()
    {
        float firstDelay = Random.Range(firstStormMinDelay, firstStormMaxDelay);

        yield return new WaitForSeconds(firstDelay);

        while (automaticStorms)
        {
            StartStorm();

            while (stormActive)
            {
                yield return null;
            }

            float nextDelay = Random.Range(stormMinDelay, stormMaxDelay);

            yield return new WaitForSeconds(nextDelay);
        }

        stormScheduleCoroutine = null;
    }

    public void StartDefeatRain()
    {
        StartStormVisuals();
    }

    public void StartStorm()
    {
        if (stormActive) return;
        if (stormCoroutine != null) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStormMessage();
        }

        stormCoroutine = StartCoroutine(StormRoutine());
    }

    private IEnumerator StormRoutine()
    {
        stormActive = true;

        StartStormVisuals();
        ApplyStormDebuff();

        float currentStormDuration = Random.Range(stormMinDuration, stormMaxDuration);

        yield return new WaitForSeconds(currentStormDuration);

        RemoveStormDebuff();
        StopStormVisuals();

        stormActive = false;
        stormCoroutine = null;
    }

    private void StartStormVisuals()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartStormAudio();
        }

        if (rainParticles != null)
        {
            var main = rainParticles.main;
            main.useUnscaledTime = true;

            rainParticles.gameObject.SetActive(true);
            rainParticles.Play();
        }

        if (stormOverlay != null)
        {
            stormOverlay.gameObject.SetActive(true);

            Color color = stormOverlay.color;
            color.a = overlayAlpha;
            stormOverlay.color = color;
        }
    }

    private void StopStormVisuals()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopStormAudio();
        }

        if (rainParticles != null)
        {
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rainParticles.gameObject.SetActive(false);
        }

        if (stormOverlay != null)
        {
            stormOverlay.gameObject.SetActive(false);
        }
    }

    private void ApplyStormDebuff()
    {
        originalSpeeds.Clear();

        AgentMovement[] colonists = FindObjectsByType<AgentMovement>(FindObjectsSortMode.None);

        foreach (AgentMovement movement in colonists)
        {
            if (movement == null) continue;

            float originalSpeed = movement.GetSpeed();
            originalSpeeds[movement] = originalSpeed;

            movement.SetSpeed(originalSpeed*stormSpeedMultiplier);
        }
    }

    private void RemoveStormDebuff()
    {
        foreach (var pair in originalSpeeds)
        {
            AgentMovement movement = pair.Key;
            float originalSpeed = pair.Value;

            if (movement != null) movement.SetSpeed(originalSpeed);
        }

        originalSpeeds.Clear();
    }
}
