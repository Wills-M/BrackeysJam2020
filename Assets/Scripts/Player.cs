using System;
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
            if (Input.GetKeyDown(KeyCode.A))
            {
                Vector2 newPosition = TryMove(new Vector2(-1, 0));
                if (newPosition != Vector2.zero)
                {
                    turn = delegate { Move(newPosition); };
                    waitingForInput = false;
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Vector2 newPosition = TryMove(new Vector2(1, 0));
                if (newPosition != Vector2.zero)
                {
                    turn = delegate { Move(newPosition); };
                    waitingForInput = false;
                }
            }
            // End round when player presses space
            if(Input.GetKeyDown(KeyCode.Space))
            {
                canPerformAction = false;
                waitingForInput = false;
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        actionQueue.Clear();
    }

    public override void Resolve()
    {
        if(canPerformAction)
        {
            // Perform player action and add to queue
            turn();
            actionQueue.Enqueue(turn);
            Debug.LogFormat("{0} actions in queue", actionQueue.Count);
        }
    }
}
