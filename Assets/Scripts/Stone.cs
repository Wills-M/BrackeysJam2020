using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Actor
{
    [SerializeField]
    private bool resetPosition;

    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    public bool TryPush(Vector2 direction)
    {
        task = new MoveTask(this, direction);
        bool canPerform = task.CanPerform();
        if (!canPerform)
            task = null;
        return canPerform;
    }

    public override void Reset()
    {
        base.Reset();
        if (resetPosition)
            transform.position = initialPosition;
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
