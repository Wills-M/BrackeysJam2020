using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [HideInInspector]
    public bool waitingForInput = true;

    private LayerMask interactableMask;

    private KeyCode cachedInput = KeyCode.None;
    private IEnumerator inputCacheCoroutine;

    private void Awake()
    {
        interactableMask = LayerMask.GetMask("Interactable");
    }

    //private Action turn; //TODO: Wrap this in it's own class so we can store a list of them on the ghost later

    private void Update()
    {
        TryCacheInput();

        if (waitingForInput && !IsPerformingTask)
        {
            // End round when player presses space
            if (cachedInput == KeyCode.Space)
            {
                cachedInput = KeyCode.None;
                canPerformAction = false;
                waitingForInput = false;

                return;
            }

            // Otherwise check for player input and attempt corresponding tasks
            if (cachedInput == KeyCode.W)
            {
                cachedInput = KeyCode.None;
                Task moveTask = new MoveTask(this, Vector2.up);
                if (moveTask.CanPerform())
                {
                    task = moveTask;
                    waitingForInput = false;
                }
            }
            else if (cachedInput == KeyCode.A)
            {
                cachedInput = KeyCode.None;
                Task moveTask = new MoveTask(this, Vector2.left);
                if (moveTask.CanPerform()) {
                    task = moveTask;
                    waitingForInput = false;
                }
            }
            else if (cachedInput == KeyCode.S)
            {
                cachedInput = KeyCode.None;
                Task moveTask = new MoveTask(this, Vector2.down);
                if (moveTask.CanPerform())
                {
                    task = moveTask;
                    waitingForInput = false;
                }
            }
            else if (cachedInput == KeyCode.D)
            {
                cachedInput = KeyCode.None;
                Task moveTask = new MoveTask(this, Vector2.right);

                if (moveTask.CanPerform()) {
                    task = moveTask;
                    waitingForInput = false;
                }
            }
            else if (cachedInput == KeyCode.E)
            {
                cachedInput = KeyCode.None;
                Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                Collider2D result = Physics2D.OverlapPoint(pos, interactableMask);
                if (result && result.TryGetComponent(out Lever lever))
                {
                    Task leverTask = new FlipLeverTask(this, lever);
                    if (leverTask.CanPerform())
                    {
                        task = leverTask;
                        waitingForInput = false;
                    }
                }
            }
        }
    }

    public override IEnumerator Reset()
    {
        resetCoroutine = StartCoroutine(base.Reset());
        
        // Clear action queue to fill up for next ghost
        actionQueue.Clear();
        yield return null;
    }

    public override IEnumerator Resolve()
    {
        if(canPerformAction) 
        {
            // Perform task and wait until finished executing
            StartCoroutine(base.Resolve());
            
            while(task.IsExecuting)
                yield return null;

            // Add task to queue
            actionQueue.Enqueue(task);

            // Load next level if player reached the goal
            Collider2D result = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Goal"));
            if (result)
                LevelManager.Instance.NextLevel();
            else
                waitingForInput = true;
        }
    }

    private void TryCacheInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            CacheInput(KeyCode.W);
        else if (Input.GetKeyDown(KeyCode.A))
            CacheInput(KeyCode.A);
        else if (Input.GetKeyDown(KeyCode.S))
            CacheInput(KeyCode.S);
        else if (Input.GetKeyDown(KeyCode.D))
            CacheInput(KeyCode.D);

        else if (Input.GetKeyDown(KeyCode.E))
            CacheInput(KeyCode.E);
        
        if (Input.GetKeyDown(KeyCode.Space))
            CacheInput(KeyCode.Space);
    }

    private void CacheInput(KeyCode key)
    {
        if(inputCacheCoroutine != null) 
            StopCoroutine(inputCacheCoroutine);

        inputCacheCoroutine = CacheThenErase(key);
        StartCoroutine(inputCacheCoroutine);
    }

    private IEnumerator CacheThenErase(KeyCode ci)
    {
        cachedInput = ci;

        yield return new WaitForSeconds(1f);

        cachedInput = KeyCode.None;
        inputCacheCoroutine = null;
    }
}