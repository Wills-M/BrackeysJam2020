using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Ghost : Actor
{
    /// <summary>
    /// Remaining sequence of actions as a ghost
    /// </summary>
    protected Queue<Task> currentActions = new Queue<Task>();

    public override IEnumerator Reset()
    {
        resetCoroutine = StartCoroutine(base.Reset());

        currentActions = new Queue<Task>(actionQueue.ToArray());
        yield return null;
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
        // Fill actionQueue
        Task[] arr = actionQueue.ToArray();
        this.actionQueue = new Queue<Task>(arr);
    }
}