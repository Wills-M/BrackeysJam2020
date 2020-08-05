﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MoveTask : Task
{
    private Vector2 direction;
    private Vector2 lastCalculatedPosition;

    private LayerMask movementMask = LayerMask.GetMask("BlocksMovement");
    private LayerMask stoneMask = LayerMask.GetMask("BlocksStone");
    private LayerMask fallMask = LayerMask.GetMask("BlocksFall");

    public MoveTask(Actor actor, Vector2 direction)
    {
        this.actor = actor;
        this.direction = direction;
    }

    public override IEnumerator Execute()
    {
        Vector2 startPos = actor.transform.position;
        for(float t = 0; t < 1; t+= Time.deltaTime * actor.taskSpeed)
        {
            actor.transform.position = Vector2.Lerp(startPos, lastCalculatedPosition, t);
            yield return null;
        }
        actor.transform.position = lastCalculatedPosition;
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
        if (direction == Vector2.right || direction == Vector2.left)
            return HorizontalTryMove(actor, direction);
        else if (direction == Vector2.up || direction == Vector2.down)
            return VerticalTryMove(actor, direction);
        else
            throw new System.Exception("Unexpected code path reached - TryMove called with unexpected direction vector.");
    }

    private Vector2 VerticalTryMove(Actor actor, Vector2 direction)
    {
        // If actor is trying to go up, check if they're on a ladder
        if (direction == Vector2.up)
        {
            Vector2 pos = (Vector2)actor.transform.position;
            Collider2D result = Physics2D.OverlapPoint(pos, fallMask);

            // If they're on a ladder than they can go up so return direction 
            if (result?.tag == "Ladder")
                return pos + direction;
            // If they're not on a ladder than movement fails so return vector2.zero
            else
                return Vector2.zero;
        }
        // If actor is trying to go down, check if they're above a ladder
        else if (direction == Vector2.down)
        {
            Vector2 offsetPosition = (Vector2)actor.transform.position + direction;
            Collider2D result = Physics2D.OverlapPoint(offsetPosition, fallMask);

            // If they're above a ladder than they can go down so return direction
            if (result?.tag == "Ladder")
                return offsetPosition;
            // If they're not above a ladder than movement fails so return vector2.zero
            else
                return Vector2.zero;
        }
        else
        {
            throw new System.Exception("Unexpected code path reached - VerticalTryMove called with unexpected direction vector.");
        }
    }

    private Vector2 HorizontalTryMove(Actor actor, Vector2 direction)
    {
        // Check for block in offset position
        Vector2 offsetPosition = (Vector2)actor.transform.position + direction;
        Collider2D result = Physics2D.OverlapPoint(offsetPosition, movementMask | stoneMask);

        // If not walking into a wall/block
        if (!result)
        {
            // If actor walking over a ledge, calculate where they will land
            if (IsAboveGround(result, offsetPosition))
                return TryFallDownGap(result, offsetPosition);
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
                    if (Physics2D.OverlapPoint(offsetPosition + Vector2.up, movementMask) == null)
                        offsetPosition += Vector2.up;
                    else offsetPosition = Vector2.zero;
                }
            }
            else offsetPosition = Vector2.zero;
        }
        return offsetPosition;
    }

    /// <summary>
    /// Returns true if position is > 1 tile above terrain
    /// </summary>
    private bool IsAboveGround(Collider2D result, Vector2 position)
    {
        // Check for ground tile in case actor is moving over edge
        result = Physics2D.OverlapPoint(position + Vector2.down, movementMask);
        return !result;
    }

    /// <summary>
    /// Returns landing position from falling down gap, or zero vector if gap is too high
    /// </summary>
    private Vector2 TryFallDownGap(Collider2D result, Vector2 offsetPosition)
    {
        // Check for ground tile in case actor is moving over edge
        result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, movementMask | fallMask);
        int fallCheck = 0;
        while (!result && fallCheck < Actor.MaxFallCheck)
        {
            fallCheck++;
            offsetPosition += Vector2.down;
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, movementMask | fallMask);
        }

        // If max fall check was reached then kill the actor and return zero vector
        if (fallCheck == Actor.MaxFallCheck)
        {
            //TODO: Kill the actor
            offsetPosition = Vector2.zero;
        }

        return offsetPosition;
    }

    private bool IsPlayerOrGhost(GameObject obj)
    {
        return obj.TryGetComponent(out Player player) || obj.TryGetComponent(out Ghost ghost);
    }
}
