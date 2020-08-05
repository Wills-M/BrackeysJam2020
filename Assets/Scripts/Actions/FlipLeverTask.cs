using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipLeverTask : Task
{
    private Lever lever;

    public FlipLeverTask(Actor actor, Lever lever)
    {
        this.actor = actor;
        this.lever = lever;
    }

    public override bool CanPerform()
    {
        // TODO: Actually check whether this can be performed
        return true;
    }

    public override void Execute()
    {
        lever.Flip();
    }

}
