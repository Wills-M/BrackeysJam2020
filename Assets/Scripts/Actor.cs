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
    /// Collection of actions to perform as a ghost
    /// </summary>
    public Queue<Task> actionQueue = new Queue<Task>();

    protected Task turn;

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
    public abstract void Resolve();

}
