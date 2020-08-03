using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    [SerializeField]
    private Player player;

    private IEnumerator turnCoroutine;

    private void Start()
    {
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
    }

    private IEnumerator TurnPhase()
    {
        while (player.waitingForInput)
        {
            yield return null;
        }
        ResolvePhase();
    }

    private void ResolvePhase()
    {
        player.Resolve();
        player.waitingForInput = true;
        turnCoroutine = TurnPhase();
        StartCoroutine(turnCoroutine);
        //TODO: Resolve all objects and player ghost
    }
}
