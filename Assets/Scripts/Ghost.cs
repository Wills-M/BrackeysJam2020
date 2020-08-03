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
        currentActions = actionQueue;
    }

    public override void Resolve()
    {
        // Perform action and add to queue
        turn = currentActions.Dequeue();
        turn();
    }

    public void InitializeActions(Queue<Action> actionQueue)
    {
        this.actionQueue = actionQueue;

        // Fill currentActions
        currentActions = actionQueue;
    }
}