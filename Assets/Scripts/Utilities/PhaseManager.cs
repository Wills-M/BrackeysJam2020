using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhaseManager : Singleton<PhaseManager>
{
    public Player player;

    [Header("Rewind Settings")]

    /// <summary>
    /// Speed to reset objects to original positions at
    /// </summary>
    [Range(0, 10)]
    public float resetSpeed;

    [SerializeField]
    [Range(0, 10)]
    private float fadeInSpeed;

    [SerializeField]
    private AnimationCurve fadeInCurve;

    [Header("Movement Settings")]
    [Range(0, 10)]
    /// <summary>
    /// Base speed for falling objects
    /// </summary>
    public float gravitySpeed;

    /// <summary>
    /// Curve used for animating gravity
    /// </summary>
    public AnimationCurve gravityCurve;

    /// <summary>
    /// Curve used for animating actor movement (besides falling)
    /// </summary>
    public AnimationCurve moveAnimationCurve;

    [Header("Prefab References")]
    [SerializeField]
    private Actor ghostPrefab;

    [SerializeField]
    private Stone timeCubePrefab;

    private List<Actor> actors;
    private List<Stone> stones;

    /// <summary>
    /// List of Time Cube starting positions
    /// </summary>
    private List<Vector2> timeCubeSpawns;

    private IEnumerator turnCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // Initialize empty actors list
        actors = new List<Actor>() { player };

        // Track all stones in level
        stones = FindObjectsOfType<Stone>().ToList();

        // Store all time cube respawn positions
        timeCubeSpawns = new List<Vector2>();
        foreach (Stone stone in stones)
        {
            if (!stone.resetPosition)
                timeCubeSpawns.Add(stone.transform.position);
        }
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
        
        // Drop any floating actors to the ground
        dropFloatingActors = StartCoroutine(DropFloatingActors());
        while (dropFloatingActors != null)
            yield return null;

        // Start a new round when player can't perform actions anymore
        // TODO: continue to let ghosts perform remaining actions?
        player.waitingForInput = true;
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    private Coroutine dropFloatingActors;

    /// <summary>
    /// Drops any floating actors to the ground
    /// </summary>
    private IEnumerator DropFloatingActors()
    {
        List<Actor> falling = new List<Actor>();
        foreach (Actor actor in actors.Concat(stones).OrderBy(i => i.transform.position.y))
        {
            if (MoveTask.IsFloating(actor))
            {
                StartCoroutine(actor.DropIfFalling());
                falling.Add(actor);

                while (actor.IsPerformingTask)
                    yield return null;
            }
        }
        dropFloatingActors = null;
    }

    public void ResolveActor(Actor actor)
    {
        StartCoroutine(actor.Resolve());
    }

    private IEnumerator ResetRound()
    {
        PostProcessingManager.Instance.RewindStart();

        // Instantiate ghost with player's action queue
        Ghost ghost = Instantiate(ghostPrefab) as Ghost;
        ghost.InitializeActions(player.actionQueue);
        actors.Add(ghost);

        // Ghost floats from player's last position to starting position
        ghost.transform.position = player.transform.position;
        ghost.initialPosition = player.initialPosition;

        // Make each task point to the ghost actor instead of player
        foreach (Task task in ghost.actionQueue)
        {
            task.actor = ghost;
        }

        // Rewind all actors and stones to starting positions
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
        
        // Respawn any time cubes that moved from their starting positions
        respawnTimeCubes = StartCoroutine(RespawnTimeCubes());
        while (respawnTimeCubes != null)
            yield return null;

        PostProcessingManager.Instance.RewindEnd();

        player.waitingForInput = true;
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    Coroutine respawnTimeCubes;
    private IEnumerator RespawnTimeCubes()
    {
        Collider2D timeCube;
        foreach(Vector2 spawn in timeCubeSpawns)
        {
            // If timecube not found at its starting position, spawn a new copy
            timeCube = Physics2D.OverlapPoint(spawn, LayerMask.GetMask("BlocksMovement"));
            if (!timeCube)
            {
                Stone copy = Instantiate(timeCubePrefab, spawn, stones[0].transform.rotation, stones[0].transform.parent);
                stones.Add(copy);

                // Fade sprite alpha from 0 to 1
                Color color = copy.spriteRenderer.color;
                color.a = 0;
                for(float t = 0; t < 1; t+= fadeInSpeed * Time.deltaTime)
                {
                    color.a = fadeInCurve.Evaluate(t);
                    copy.spriteRenderer.color = color;
                    yield return null;
                }
                color.a = 1;
            }
        }

        respawnTimeCubes = null;
    }
}
