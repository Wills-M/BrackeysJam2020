using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MoveTask : Task
{
    private Vector2 direction;
    private Vector2 lastCalculatedPosition;

    private LayerMask terrainMask = LayerMask.GetMask("Terrain");

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
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, terrainMask);
            int fallCheck = 0;
            while (!result && fallCheck < Actor.MaxFallCheck)
            {
                fallCheck++;
                offsetPosition += Vector2.down;
                result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, terrainMask);
            }

            // If max fall check was reached then kill the actor and return zero vector
            if (fallCheck == Actor.MaxFallCheck)
            {
                //TODO: Kill the actor
                offsetPosition = Vector2.zero;
            }
            return offsetPosition;
        }
        else
        {
            // If moving against a stone, try to push it
            if (result.gameObject.TryGetComponent(out Stone stone))
            {
                if (stone.TryPush(direction))
                    return offsetPosition;
            }

            // If Actor perfomring task isn't a Stone, try to move
            var type = actor as Stone;
            if (type == null)
            {
                // If actor is walking into a terrain element (stone or level tile)
                if (!IsPlayerOrGhost(result.gameObject))
                {
                    // Check if spot above is empty and move it there if it is
                    if (Physics2D.OverlapPoint(offsetPosition + Vector2.up, terrainMask) == null)
                        offsetPosition += Vector2.up;
                }
                return offsetPosition;
            }
        }

        // If there was something blocking the player, return zero vector
        return Vector2.zero;
    }

    protected void Move(Actor actor, Vector2 newPosition)
    {
        actor.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }

    private bool IsPlayerOrGhost(GameObject obj)
    {
        return obj.TryGetComponent(out Player player) || obj.TryGetComponent(out Ghost ghost);
    }
}
