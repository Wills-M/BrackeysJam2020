using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [HideInInspector]
    public bool waitingForInput = true;

    //private Action turn; //TODO: Wrap this in it's own class so we can store a list of them on the ghost later

    private void Update()
    {
        if (waitingForInput)
        {
            // End round when player presses space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                canPerformAction = false;
                waitingForInput = false;

                return;
            }

            // Otherwise check for player input and attempt corresponding tasks
            if (Input.GetKeyDown(KeyCode.A))
            {
                Task moveTask = new MoveTask(this, Vector2.left);
                if (moveTask.CanPerform()) {
                    turn = moveTask;
                    waitingForInput = false;
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Task moveTask = new MoveTask(this, Vector2.right);

                if (moveTask.CanPerform()) {
                    turn = moveTask;
                    waitingForInput = false;
                }
            }

        }
    }

    public override void Reset()
    {
        base.Reset();
        actionQueue.Clear();

        // Move to starting position
        transform.position = PhaseManager.start;
    }

    public override void Resolve()
    {
        if(canPerformAction)
        {
            // Perform player action and add to queue
            turn.Execute();
            actionQueue.Enqueue(turn);

            // TODO: Should be checking for current position instead of below but move currently goes on top of the goal
            Vector2 pos = new Vector2(transform.position.x, transform.position.y) + Vector2.down;
            Collider2D result = Physics2D.OverlapPoint(pos);
            if (result.tag == "Finish")
            {
                LevelManager.Instance.NextLevel();
            }

            Debug.LogFormat("{0} actions in queue", actionQueue.Count);
        }
    }
}
