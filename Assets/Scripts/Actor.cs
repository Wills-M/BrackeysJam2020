using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    private const int MaxFallCheck = 50;

    /// <summary>
    /// Collection of actions to perform as a ghost
    /// </summary>
    public Queue<Action> actionQueue = new Queue<Action>();

    protected Action turn;

    /// <summary>
    /// True until Actor finishes their round (i.e. dies, ends round)
    /// </summary>
    public bool canPerformAction { get; protected set; }
    
    void Start()
    {
        Reset();
    }

    /// <summary>
    /// Initializes actor at beginning state of level
    /// </summary>
    public virtual void Reset()
    {
        canPerformAction = true;

        // Move to starting position
        transform.position = PhaseManager.start;
    }

    /// <summary>
    /// Resolves Actor's action
    /// </summary>
    public abstract void Resolve();

    /// <summary>
    /// Returns Vector2 for new position if move succeeds. If move fails returns Vector2.zero.
    /// </summary>
    protected Vector2 TryMove(Vector2 direction)
    {
        // Check for block in offset position
        Vector2 offsetPosition = new Vector2(transform.position.x, transform.position.y) + direction;
        Collider2D result = Physics2D.OverlapPoint(offsetPosition);

        // If there was no collider check for block to hold actor up
        if (!result)
        {
            // As long as spot below current offset is empty move it down
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            int fallCheck = 0;
            while (!result && fallCheck < MaxFallCheck)
            {
                fallCheck++;
                offsetPosition += Vector2.down;
                result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            }

            // If max fall check was reached then kill the actor and return empty vector3
            if (fallCheck == MaxFallCheck)
            {
                //TODO: Kill the actor
                return Vector2.zero;
            }
            // Else return the offset
            else
            {
                return offsetPosition;
            }
        }
        else
        {
            // Check if spot above is empty and move it there if it is
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.up);
            if (!result)
            {
                offsetPosition += Vector2.up;
                return offsetPosition;
            }
        }

        // If there was something blocking the player then return empty vector3
        return Vector2.zero;
    }

    protected void Move(Vector2 newPosition)
    {
        transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

}
