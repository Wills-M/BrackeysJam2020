using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhaseManager : Singleton<PhaseManager>
{
    public Player player;

    /// <summary>
    /// Speed to reset objects to original positions at
    /// </summary>
    [Range(0, 10)]
    public float resetSpeed;

    [SerializeField]
    private Actor ghostPrefab;

    private List<Actor> actors;
    private List<Stone> stones;

    private IEnumerator turnCoroutine;

    protected override void Awake()
    {
        base.Awake();

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
        if (!player.canPerformAction)
        {
            StartCoroutine(ResetRound());
        }
        else
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
            ResolveActor(actor);
            
            // Wait for each actor to finish their task before playing the next one
            while (actor.IsPerformingTask)
                yield return null;
        }

        // Start a new round when player can't perform actions anymore
        // TODO: continue to let ghosts perform remaining actions?


        player.waitingForInput = true;
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    public void ResolveActor(Actor actor)
    {
        StartCoroutine(actor.Resolve());
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
        
        player.waitingForInput = true;
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }
}
