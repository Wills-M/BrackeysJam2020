using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Actor : MonoBehaviour
{
    public static readonly int MaxFallCheck = 50;

    /// <summary>
    /// True if Actor is a Player or Ghost, false otherwise (ex. Stone)
    /// </summary>
    public bool IsCharacter;

    /// <summary>
    /// Returns true when actor is in the middle of executing a task, false otherwise
    /// </summary>
    public bool IsPerformingTask { 
        get {
            if (task != null)
                return task.IsExecuting;
            else 
                return false;
        } 
    }
    public bool IsResetting { get => resetCoroutine != null; }

    /// <summary>
    /// Collection of actions to perform as a ghost
    /// </summary>
    public Queue<Task> actionQueue = new Queue<Task>();

    /// <summary>
    /// Task to be executed in Resolve()
    /// </summary>
    protected Task task;

    [Range(1, 10)]
    public float taskSpeed;
    
    /// <summary>
    /// Curve used for animating all actor movement/task execution
    /// </summary>
    public AnimationCurve taskAnimationCurve;

    /// <summary>
    /// True until Actor finishes their round (i.e. dies, ends round)
    /// </summary>
    public bool canPerformAction;

    protected Coroutine resetCoroutine;

    public Vector3 initialPosition;

    public enum Direction { LEFT, RIGHT }

    public Direction direction = Direction.RIGHT;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        canPerformAction = true;

        // Don't set ghost's initial position on spawn (set via PhaseManager.ResetRound() instead)
        if(this as Ghost == null)
            initialPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initializes actor at beginning state of level
    /// </summary>
    public virtual IEnumerator Reset()
    {
        Debug.LogFormat("{0}.Reset() - moving actor to starting position, enabling actions", name);
        canPerformAction = true;

        // Set to starting direction
        if (IsCharacter)
            SetDirection(Direction.RIGHT);

        // Move to starting position
        Vector2 startPos = transform.position;
        for (float t = 0; t < 1; t += Time.deltaTime * taskSpeed) {
            transform.position = Vector2.Lerp(startPos, initialPosition, taskAnimationCurve.Evaluate(t));
            yield return null;
        }
        transform.position = initialPosition;

        resetCoroutine = null;
    }

    /// <summary>
    /// Resolves Actor's action
    /// </summary>
    public virtual IEnumerator Resolve()
    {
        // Make sure actor can perform a task, and isn't in the middle of one already 
        if(canPerformAction && !IsPerformingTask)
        {
            StartCoroutine(task.Execute());

            // Wait until task is completed
            while (task.IsExecuting)
                yield return null;
        }
    }

    public void SetDirection(Direction direction)
    {
        if (direction == Direction.LEFT)
            spriteRenderer.flipX = true;
        else if(direction == Direction.RIGHT)
            spriteRenderer.flipX = false;
    }
}
