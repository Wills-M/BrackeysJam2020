using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MoveTask : Task
{
    private Vector2 direction;
    private Vector2 lastCalculatedPosition;

    public MoveTask(Actor actor, Vector2 direction)
    {
        this.actor = actor;
        this.direction = direction;
    }

    public override void Execute()
    {
        Move(actor, lastCalculatedPosition);
    }

    public override bool CanPerform()
    {
        lastCalculatedPosition = TryMove(actor, direction);
        return lastCalculatedPosition != Vector2.zero;
    }

    /// <summary>
    /// Returns Vector2 for new position if move succeeds. If move fails returns Vector2.zero.
    /// Has the side effect of assigning rock turns so must be called before actually moving
    /// </summary>
    protected Vector2 TryMove(Actor actor, Vector2 direction)
    {
        // Check for block in offset position
        Vector2 offsetPosition = new Vector2(actor.transform.position.x, actor.transform.position.y) + direction;
        Collider2D result = Physics2D.OverlapPoint(offsetPosition);

        // If there was no collider check for block to hold actor up
        if (!result)
        {
            // As long as spot below current offset is empty move it down
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            int fallCheck = 0;
            while (!result && fallCheck < Actor.MaxFallCheck)
            {
                fallCheck++;
                offsetPosition += Vector2.down;
                result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            }

            // If max fall check was reached then kill the actor and return empty vector3
            if (fallCheck == Actor.MaxFallCheck)
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
            // Check if collision is with a stone and try to move that stone
            Stone stone = null;
            if (result.gameObject.TryGetComponent<Stone>(out stone))
            {
                if (stone.TryPush(direction))
                {
                    return offsetPosition;
                }
            }

            // Check if actor currently running this task is itself a stone
            // If it isn't than try to go up a block
            var type = actor as Stone;
            if (type == null)
            {
                // Check if spot above is empty and move it there if it is
                result = Physics2D.OverlapPoint(offsetPosition + Vector2.up);
                if (!result)
                {
                    offsetPosition += Vector2.up;
                    return offsetPosition;
                }
            }
        }

        // If there was something blocking the player then return empty vector3
        return Vector2.zero;
    }

    protected void Move(Actor actor, Vector2 newPosition)
    {
        actor.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }
}
