using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{

    protected void Move(Vector2 direction)
    {
        // Check for block in offset position
        Vector2 offsetPosition = new Vector2(transform.position.x, transform.position.y) + direction;
        Collider2D result = Physics2D.OverlapPoint(offsetPosition);

        // If there was no collider check for block to hold actor up
        if (!result)
        {
            // As long as spot below current offset is empty move it down
            result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            while (!result)
            {
                offsetPosition += Vector2.down;
                result = Physics2D.OverlapPoint(offsetPosition + Vector2.down);
            }

            // Move Actor
            transform.position = new Vector3(offsetPosition.x, offsetPosition.y);
        }
    }

}
