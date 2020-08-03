using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    [SerializeField]
    private Player player;

    private List<Actor> actors;

    private IEnumerator turnCoroutine;

    /// <summary>
    /// Starting position for actors
    /// </summary>
    public static Vector2 start;

    private void Awake()
    {
        // Store player starting position
        start = player.transform.position;

        // Initialize actors list with player
        actors = new List<Actor>() { player };
    }

    private void Start()
    {
        // Initiate action/resolve phase loop
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    /// <summary>
    /// Waits for player before resolving actions in ResolvePhase()
    /// </summary>
    private IEnumerator TurnPhase()
    {
        while (player.waitingForInput)
        {
            yield return null;
        }
        ResolvePhase();
    }

    /// <summary>
    /// Resolves each actor's actions before looping back to TurnPhase()
    /// </summary>
    private void ResolvePhase()
    {
        // Resolve each actor's action
        foreach(Actor actor in actors)
        {
            actor.Resolve();
        }
        player.waitingForInput = true;

        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }
}
