using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class WaitTask : Task
{
    private float delay;

    public WaitTask(float delay = 0f)
    {
        this.delay = delay;
    }

    public override bool CanPerform()
    {
        return true;
    }

    public override IEnumerator Execute()
    {
        // Wait for delay before ending execution
        IsExecuting = true;
        yield return new WaitForSeconds(delay);
        IsExecuting = false;
    }
}
