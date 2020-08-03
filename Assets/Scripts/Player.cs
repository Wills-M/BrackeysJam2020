using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [HideInInspector]
    public bool waitingForInput = true;

    private Action turn; //TODO: Wrap this in it's own class so we can store a list of them on the ghost later

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
        }
    }

    public void Resolve()
    {
        turn();
    }

}
