﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MoveTask : Task
{
    private Vector2 direction;
    private Vector2 lastCalculatedPosition;

    private LayerMask movementMask = LayerMask.GetMask("BlocksMovement");
    private LayerMask stoneMask = LayerMask.GetMask("BlocksStone");
    private LayerMask ladderMask = LayerMask.GetMask("BlocksFall");

    private readonly float squishAmount = 0.3f;

    /// <summary>
    /// True if moving against a block this execution
    /// </summary>
    private bool pushing = false;

    /// <summary>
    /// List of stones actor is trying to push this execution
    /// </summary>
    private List<Stone> stonesToPush;

    public MoveTask(Actor actor, Vector2 direction)
    {
        this.actor = actor;
        this.direction = direction;
    }

    public override IEnumerator Execute()
    {
        IsExecuting = true;

        // Set direction for player/ghosts based on movement
        SetDirection();

        // Select proper animation for character actors
        AnimComponent.AnimID animID = SelectAnimation();

        // Get material string based on what is being done
        string matString = "";
        if (animID == AnimComponent.AnimID.Pushing)
            matString = "_HorizontalScale";
        else
            matString = "_VerticalScale";

        // Play animation for characters
        if (actor.TryGetComponent(out AnimComponent animComponent))
            animComponent.SetAnimation(animID, true);

        
        float speed = actor.taskSpeed;
        if (actor.IsCharacter && pushing) {
            // Scale speed for pushing
            speed *= actor.pushSpeedScalar;

            // Start moving pushed stones
            foreach (Stone stone in stonesToPush) {
                stone.taskSpeed = speed;
                PhaseManager.Instance.ResolveActor(stone);
            }
        }

        // Get material for animating squish
        Material mat = actor.spriteRenderer.material;

        // Lerp actor to new position
        Vector2 startPos = actor.transform.position;
        for(float t = 0; t < 1; t+= Time.deltaTime * speed)
        {
            float eval = actor.taskAnimationCurve.Evaluate(t);
            // Squish animation
            float x = Mathf.Sin(eval * Mathf.PI);
            mat.SetFloat(matString, 1f - (x * squishAmount));

            // Position move animation
            actor.transform.position = Vector2.Lerp(startPos, lastCalculatedPosition, eval);
            yield return null;
        }
        actor.transform.position = lastCalculatedPosition;

        // Stop animation
        if (actor.IsCharacter)
            animComponent.SetAnimation(animID, false);

        if(pushing)
        {
            // Wait until every stone is finished being pushed
            while(stonesToPush.Count > 0)
            {
                if (!stonesToPush[0].IsPerformingTask)
                    stonesToPush.RemoveAt(0);
                yield return null;
            }
        }

        IsExecuting = false;
    }

    /// <summary>
    /// Returns animation to play for player/ghost actors
    /// </summary>
    private AnimComponent.AnimID SelectAnimation()
    {
        // Don't animate stones
        if (!actor.IsCharacter)
            return AnimComponent.AnimID.None;

        else if (pushing)
            return AnimComponent.AnimID.Pushing;
        // TODO: check if we're falling and use that animation
        else return AnimComponent.AnimID.Moving;
    }

    private void SetDirection()
    {
        if (actor.IsCharacter)
        {
            if (direction == Vector2.left)
                actor.SetDirection(Actor.Direction.LEFT);
            else if (direction == Vector2.right)
                actor.SetDirection(Actor.Direction.RIGHT);
        }
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
            Collider2D result = Physics2D.OverlapPoint(pos, ladderMask);

            // If they're on a ladder than they can go up so return direction 
            if (result?.tag == "Ladder")
            {
                // Move up if there's a ladder, or space above one
                Vector2 above = pos + Vector2.up;
                if (!Physics2D.OverlapPoint(above, movementMask) || Physics2D.OverlapPoint(above, ladderMask))
                    return pos + direction;
                else
                    return Vector2.zero;
            }
            // If they're not on a ladder than movement fails so return vector2.zero
            else
                return Vector2.zero;
        }
        // If actor is trying to go down, check if they're above a ladder
        else if (direction == Vector2.down)
        {
            Vector2 belowActor = (Vector2)actor.transform.position + direction;

            // Check for ladder and/or terrain below
            Collider2D ladderBelow = Physics2D.OverlapPoint(belowActor, ladderMask);
            Collider2D terrainBelow = Physics2D.OverlapPoint(belowActor, movementMask);

            // If they're dropping off the bottom of a ladder above ground, return position at ground below
            if (DroppingOffBottomOfLadder(ladderBelow, terrainBelow))
                return TryFallDownGap(actor.transform.position);
            // If they're above a ladder, return its position
            else if (ladderBelow && !terrainBelow)
                return belowActor;
            else return Vector2.zero;

        }
        else
        {
            throw new System.Exception("Unexpected code path reached - VerticalTryMove called with unexpected direction vector.");
        }
    }

    /// <summary>
    /// Returns true when at the bottom of a ladder above ground
    /// </summary>
    /// <param name="ladderBelowActor"></param>
    /// <param name="terrainBelowActor"></param>
    /// <returns></returns>
    private bool DroppingOffBottomOfLadder(Collider2D ladderBelowActor, Collider2D terrainBelowActor)
    {
        return !terrainBelowActor && !ladderBelowActor && IsAboveGround(actor.transform.position);
    }

    private Vector2 HorizontalTryMove(Actor actor, Vector2 direction)
    {
        // Check for block in offset position
        Vector2 offsetPosition = (Vector2)actor.transform.position + direction;
        Collider2D blockingTile = Physics2D.OverlapPoint(offsetPosition, movementMask | stoneMask);

        // If not walking into a wall/stone
        if (!blockingTile)
        {
            pushing = false;

            // If walking into a ladder, return its position
            if (Physics2D.OverlapPoint(offsetPosition, ladderMask))
                return offsetPosition;
            // If actor walking over a ledge, calculate where they will land
            else if (IsAboveGround(offsetPosition))
                return TryFallDownGap(offsetPosition);
        }
        else
        {
            // If moving against a stone, try to push it
            bool movingAgainstStone = blockingTile.gameObject.TryGetComponent(out Stone stone);
            pushing = movingAgainstStone && stone.TryPush(direction);
            if (pushing) {
                // Collect all stones this actor is pushing
                stonesToPush = GetStonesInDirection(actor, direction);
                Debug.LogFormat("{0} will push {1} stones towards {2}", actor.name, stonesToPush.Count, direction);
                return offsetPosition;
            }

            // If Actor perfomring task isn't a Stone, try to move
            var type = actor as Stone;
            if (type == null)
            {
                // If actor is walking into a terrain element (stone or level tile)
                if (!IsPlayerOrGhost(blockingTile.gameObject))
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
    /// Returns list of stones that would be pushed if actor moved in direction
    /// </summary>
    private List<Stone> GetStonesInDirection(Actor actor, Vector2 direction)
    {
        List<Stone> stones = new List<Stone>();
        Collider2D result;

        // Check each tile in direction of movement
        Vector2 offset = actor.transform.position;
        do {
            offset += direction;

            // If tile is stone, add to list
            result = Physics2D.OverlapPoint(offset, movementMask | stoneMask);
            if (result != null && result.TryGetComponent(out Stone stone))
                stones.Add(stone);

        } while (result != null);

        return stones;
    }

    /// <summary>
    /// Returns true if position is > 1 tile above terrain
    /// </summary>
    private bool IsAboveGround(Vector2 position)
    {
        // Check for ground tile in case actor is moving over edge
        Collider2D result = Physics2D.OverlapPoint(position + Vector2.down, movementMask);
        return !result;
    }

    /// <summary>
    /// Returns landing position from falling down gap, or zero vector if gap is too high
    /// </summary>
    private Vector2 TryFallDownGap(Vector2 offsetPosition)
    {
        // Stones fall down ladders, player/ghosts don't
        int layerMask = actor.IsCharacter ? movementMask | ladderMask : (int)movementMask;

        // Check for ground tile in case actor is moving over edge
        Collider2D result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, layerMask);
        int fallCheck = 0;
        while (!result && fallCheck < Actor.MaxFallCheck)
        {
            fallCheck++;
            offsetPosition += Vector2.down;
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down, layerMask);
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
