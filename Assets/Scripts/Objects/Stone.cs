using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Actor
{
    /// <summary>
    /// True for normal stones that rewind to original positions, 
    /// false for time cubes that stay in place
    /// </summary>
    public bool resetPosition;

    public bool TryPush(Vector2 direction)
    {
        task = new MoveTask(this, direction);
        bool canPerform = task.CanPerform();
        if (!canPerform)
            task = null;
        return canPerform;
    }

    public override IEnumerator Reset()
    {
        if (resetPosition)
            resetCoroutine = StartCoroutine(base.Reset());

        yield return null;
    }

    public override IEnumerator Resolve()
    {
        if (task != null && !IsPerformingTask)
        {
            // Perform task and wait until finished
            StartCoroutine(task.Execute());
            while(task.IsExecuting)
                yield return null;

            task = null;
        }
    }
}
