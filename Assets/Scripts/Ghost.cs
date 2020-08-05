﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Ghost : Actor
{
    /// <summary>
    /// Remaining sequence of actions as a ghost
    /// </summary>
    protected Queue<Task> currentActions = new Queue<Task>();

    public override void Reset()
    {
        base.Reset();
        currentActions = new Queue<Task>(actionQueue.ToArray());

        // Move to starting position
        transform.position = PhaseManager.start;
    }

    public override IEnumerator Resolve()
    {
        if(!IsPerformingTask && canPerformAction)
        {
            // Perform action and remove from queue
            task = currentActions.Dequeue();
            if (task.CanPerform()) 
                StartCoroutine(base.Resolve());

            while(task.IsExecuting)
                yield return null;

            // Set flag if no actions remaining
            if (currentActions.Count == 0)
                canPerformAction = false;
        }
    }

    public void InitializeActions(Queue<Task> actionQueue)
    {
        Debug.LogFormat("{0} - initializing {1} actions in queue", name, actionQueue.Count);

        // Fill actionQueue
        Task[] arr = actionQueue.ToArray();
        this.actionQueue = new Queue<Task>(arr);
    }
}