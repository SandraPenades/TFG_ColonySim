using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentNeeds : MonoBehaviour
{
    // Para que no bajen las necesidades mientras testeo
    [Header("Modo de prueba")]
    public bool developerMode = false;

    // Necesidades actuales
    [Header("Necesidades actuales")]
    public float hunger = 100f;
    public float energy = 100f;
    public float fun = 100f;
    public float social = 100f;

    // Desgaste por segundo
    [Header("Disminución de necesidades")]
    public float hungerDecayRate = 1.5f;
    public float energyDecayRate = 1.0f;
    public float funDecayRate = 0.75f;
    public float socialDecayRate = 0.75f;

    // Umbral de alerta
    [Header("Umbrales de alerta")]
    public float hungerThreshold = 30f;
    public float energyThreshold = 20f;
    public float funThreshold = 20f;
    public float socialThreshold = 20f;
    public float healthThreshold = 10f;

    // Daño y recuperación
    [Header("Daño y Recuperación")]
    public float damageNeeds = 3f;
    public float healthRecoveryRate = 1f;

    // Vida
    [Header("Vida")]
    public float health = 100f;
    public float maxhealth = 100;

    public bool IsDead { get; private set; } = false;
    private Coroutine deathCoroutine;

    void Update()
    {
        if (IsDead) return; // Si está muerto no tiene necesidades

        if (!developerMode)
        {
            // Disminuyen las necesidades con el paso del tiempo
            hunger -= hungerDecayRate * Time.deltaTime;
            energy -= energyDecayRate * Time.deltaTime;
            fun -= funDecayRate * Time.deltaTime;
            social -= socialDecayRate * Time.deltaTime;

            // Para evitar números negativos o mayores a 100
            hunger = Mathf.Clamp(hunger, 0f, 100f);
            energy = Mathf.Clamp(energy, 0f, 100f);
            fun = Mathf.Clamp(fun, 0f, 100f);
            social = Mathf.Clamp(social, 0f, 100f);

            // Si alguna necesidad está a 0, disminuye la vida
            UpdateHealth();
        }
    }

    // Funciones de aviso para el GOAP
    public bool IsHungry()
    {
        return hunger <= hungerThreshold;
    }

    public bool IsSleepy()
    {
        return energy <= energyThreshold;
    }

    public bool IsBored()
    {
        return fun <= funThreshold;
    }

    public bool IsLonely()
    {
        return social <= socialThreshold;
    }

    public bool IsDying()
    {
        return health <= healthThreshold;
    }



    // Si alguna de las necesidades se queda a 0, resta vida poco a poco
    private void UpdateHealth()
    {
        float damage = 0f;

        if (hunger <= 0f || energy <= 0f || fun <= 0f || social <= 0f) damage += damageNeeds;

        if (damage > 0f) health -= damage * Time.deltaTime;
        else health += healthRecoveryRate * Time.deltaTime;

        health = Mathf.Clamp(health, 0f, maxhealth);

        if (health <= 0f) Die();
    }

    // Si le golpea un enemigo, recibe daño y le resta vida
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        health -= amount;
        health = Mathf.Clamp(health, 0f, maxhealth);

        if (health <= 0f) Die();
    }



    // Si el colono se queda sin vida, muere y al rato desaparece el GameObject
    private void Die()
    {
        if (IsDead) return;

        IsDead = true;

        if (UIManager.Instance != null)
        {
            string colonistName = gameObject.name.Replace("Colonist_", "");

            UIManager.Instance.ShowColonistDeathMessage(colonistName);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColonistDeath();
        }

        AgentBrain brain = GetComponent<AgentBrain>();

        if (brain != null)
        {
            brain.AbortCurrentAction();
            brain.enabled = false;
        }

        AgentMovement movement = GetComponent<AgentMovement>();
        if (movement != null)
        {
            movement.StopMoving();
            movement.enabled = false;
        }

        ColonistRecruitment recruitment = GetComponent<ColonistRecruitment>();
        if (recruitment != null) recruitment.enabled = false;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.45f, 0.45f, 0.45f, 1f);

        gameObject.name += " (muerto)";

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
        }

        deathCoroutine = StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(60f);
        Destroy(gameObject);
    }
}
