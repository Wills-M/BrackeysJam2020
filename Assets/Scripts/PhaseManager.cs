using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private Actor ghostPrefab;

    private List<Actor> actors;
    private List<Stone> stones;

    private IEnumerator turnCoroutine;

    /// <summary>
    /// Starting position for actors
    /// </summary>
    public static Vector2 start;

    private void Awake()
    {
        // Store player starting position
        start = player.transform.position;

        // Initialize empty actors list
        actors = new List<Actor>() { player };
        stones = FindObjectsOfType<Stone>().ToList();
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
        StartCoroutine(ResolvePhase());
    }

    /// <summary>
    /// Resolves each actor's actions before looping back to TurnPhase()
    /// </summary>
    private IEnumerator ResolvePhase()
    {
        // Resolve each actor's action
        foreach(Actor actor in actors)
        {
            Debug.LogFormat("ResolvePhase() | resolving actor {0}", actor.name);
            StartCoroutine(actor.Resolve());
            
            // Wait for each actor to finish their task before playing the next one
            while (actor.IsPerformingTask)
                yield return null;
        }

        // Resolve stones as well (no canPerformAction check needed here)
        foreach(Stone stone in stones)
        {
            StartCoroutine(stone.Resolve());

            // Wait for each to finish their task before playing the next one
            while (stone.IsPerformingTask)
                yield return null;
        }

        // Start a new round when player can't perform actions anymore
        // TODO: continue to let ghosts perform remaining actions?
        if(!player.canPerformAction)
        {
            ResetRound();
        }

        Debug.LogFormat("ResolvePhase() | finished resolving, restoring player input");
        player.waitingForInput = true;

        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    private void ResetRound()
    {
        // Instantiate ghost with player's action queue
        Ghost ghost = Instantiate(ghostPrefab) as Ghost;
        ghost.InitializeActions(player.actionQueue);
        foreach(Task task in ghost.actionQueue)
        {
            task.actor = ghost;
        }

        // Initialize all actors
        actors.Add(ghost);
        foreach(Actor actor in actors)
        {
            actor.Reset();
        }
        foreach(Stone stone in stones)
        {
            stone.Reset();
        }
    }
}
