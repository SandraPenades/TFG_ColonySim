using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    private static readonly List<Door> allDoors = new List<Door>();
    private Collider2D doorCollider;
    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        doorCollider.isTrigger = false;
    }

    private void Start()
    {
        RefreshAllAgents();
    }

    public void RefreshAllAgents()
    {
        ColonistRecruitment[] agents = FindObjectsByType<ColonistRecruitment>(FindObjectsSortMode.None);

        foreach (ColonistRecruitment agent in agents)
        {
            SetAccessForAgent(agent.gameObject);
        }
    }

    public void SetAccessForAgent(GameObject agent)
    {
        if (agent == null || doorCollider == null) return;

        ColonistRecruitment recruitment = agent.GetComponent<ColonistRecruitment>();

        if (recruitment == null) return;

        ThiefVisitor thief = agent.GetComponent<ThiefVisitor>();

        bool isThief = thief != null && thief.enabled;

        bool canPass = recruitment.IsColonyMember || isThief;

        Collider2D[] agentColliders = agent.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in agentColliders)
        {
            if (collider == null) continue;
            if (collider == doorCollider) continue;

            Physics2D.IgnoreCollision(doorCollider, collider, canPass);
        }
    }

    // Para darle/quitarle acceso a las puertas a los visitantes
    public static void RefreshAllDoorsFor(GameObject agent)
    {
        foreach (Door door in allDoors)
        {
            if (door != null)
            {
                door.SetAccessForAgent(agent);
            }
        }
    }

    public static void RefreshAllDoors()
    {
        foreach (Door door in allDoors)
        {
            if (door != null)
            {
                door.RefreshAllAgents();
            }
        }
    }

    private void OnEnable()
    {
        if (!allDoors.Contains(this))
        {
            allDoors.Add(this);
        }

        RefreshAllAgents();
    }

    private void OnDisable()
    {
        allDoors.Remove(this);
    }
}
