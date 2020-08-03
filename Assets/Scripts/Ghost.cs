using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Ghost : Actor
{
    /// <summary>
    /// Remaining sequence of actions as a ghost
    /// </summary>
    protected Queue<Action> currentActions = new Queue<Action>();

    public override void Reset()
    {
        base.Reset();
        currentActions = new Queue<Action>(actionQueue.ToArray());
    }

    public override void Resolve()
    {
        // Perform action and remove from queue
        turn = currentActions.Dequeue();
        turn();

        // Set flag if no actions remaining
        if (currentActions.Count == 0)
            canPerformAction = false;
    }

    public void InitializeActions(Queue<Action> actionQueue)
    {
        Debug.LogFormat("{0} - initializing {1} actions in queue", name, actionQueue.Count);

        // Fill actionQueue
        Action[] arr = actionQueue.ToArray();
        this.actionQueue = new Queue<Action>(arr);
    }
}