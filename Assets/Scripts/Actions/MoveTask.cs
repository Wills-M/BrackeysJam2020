using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MoveTask : Task
{
    private Vector2 direction;
    private Vector2 lastCalculatedPosition;

    private LayerMask movementMask = LayerMask.GetMask("BlocksMovement");
    private LayerMask stoneMask = LayerMask.GetMask("BlocksStone");
    private LayerMask fallMask = LayerMask.GetMask("BlocksFall");

    private readonly float squishAmount = 0.3f;

    /// <summary>
    /// True if moving against a block this execution
    /// </summary>
    private bool pushing = false;

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

        // Play animation
        actor.TryGetComponent(out AnimComponent animComponent);
        if (actor.IsCharacter)
            animComponent.SetAnimation(animID, true);

        // Get material for animating squish
        Material mat = actor.spriteRenderer.material;

        // Lerp actor to new position
        Vector2 startPos = actor.transform.position;
        for(float t = 0; t < 1; t+= Time.deltaTime * actor.taskSpeed)
        {
            float eval = actor.taskAnimationCurve.Evaluate(t);
            // Squish animation
            float x = Mathf.Sin(eval * Mathf.PI);
            mat.SetFloat("_VerticalScale", 1f - (x * squishAmount));

            // Position move animation
            actor.transform.position = Vector2.Lerp(startPos, lastCalculatedPosition, eval);
            yield return null;
        }
        actor.transform.position = lastCalculatedPosition;

        // Stop animation
        if (actor.IsCharacter)
            animComponent.SetAnimation(animID, false);

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
            Collider2D result = Physics2D.OverlapPoint(pos, fallMask);

            // If they're on a ladder than they can go up so return direction 
            if (result?.tag == "Ladder")
            {
                Vector2 above = pos + Vector2.up;
                if (Physics2D.OverlapPoint(above, movementMask) == null)
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
            bool movingAgainstStone = result.gameObject.TryGetComponent(out Stone stone);
            pushing = movingAgainstStone && stone.TryPush(direction);
            if (pushing) {
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
