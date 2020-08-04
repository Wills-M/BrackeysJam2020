using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Actor
{
    private MoveTask moveTask;
    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    public bool TryPush(Vector2 direction)
    {
        turn = new MoveTask(this, direction);
        bool canPerform = turn.CanPerform();
        if (!canPerform)
            turn = null;
        return canPerform;
    }

    public override void Reset()
    {
        base.Reset();
        transform.position = initialPosition;
    }

    public override void Resolve()
    {
        if (turn != null)
        {
            turn.Execute();
            turn = null;
        }
    }
}
