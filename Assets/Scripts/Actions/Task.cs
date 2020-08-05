using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    /// <summary>
    /// Actor to perform the task for
    /// </summary>
    public Actor actor;

    public abstract IEnumerator Execute();

    public abstract bool CanPerform();
    
}
