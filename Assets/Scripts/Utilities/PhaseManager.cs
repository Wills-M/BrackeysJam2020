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

    private void Awake()
    {
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
            StartCoroutine(ResetRound());
        }


        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
        player.waitingForInput = true;
    }

    private IEnumerator ResetRound()
    {
        // Instantiate ghost with player's action queue
        Ghost ghost = Instantiate(ghostPrefab) as Ghost;
        ghost.InitializeActions(player.actionQueue);

        // Ghost floats from player's last position to starting position
        ghost.transform.position = player.transform.position;
        ghost.initialPosition = player.initialPosition;

        // Make each task point to the ghost actor instead of player
        foreach (Task task in ghost.actionQueue)
        {
            task.actor = ghost;
        }

        // Initialize all actors
        actors.Add(ghost);
        foreach(Actor actor in actors)
        {
            StartCoroutine(actor.Reset());
            while (actor.IsResetting)
                yield return null;
        }
        foreach(Stone stone in stones)
        {
            StartCoroutine(stone.Reset());
            while (stone.IsResetting)
                yield return null;
        }
    }
}
