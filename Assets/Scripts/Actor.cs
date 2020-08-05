using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Collection of actions to perform as a ghost
    /// </summary>
    public Queue<Task> actionQueue = new Queue<Task>();

    /// <summary>
    /// Task to be executed in Resolve()
    /// </summary>
    public Task task;

    [Range(1, 10)]
    public float taskSpeed;

    /// <summary>
    /// True until Actor finishes their round (i.e. dies, ends round)
    /// </summary>
    public bool canPerformAction;

    void Start()
    {
        Reset();
    }

    /// <summary>
    /// Initializes actor at beginning state of level
    /// </summary>
    public virtual void Reset()
    {
        Debug.LogFormat("{0}.Reset() - moving actor to starting position, enabling actions", name);

        canPerformAction = true;
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

}
