using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{

    [SerializeField]
    private int currentLevelIndex = 0;

    /// <summary>
    /// Overrides build index of next level to load. Ignored if less than 0
    /// </summary>
    [SerializeField]
    private int nextLevelOverride = -1;

    public void NextLevel()
    {
        currentLevelIndex++;
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void NextLevel(GameObject goal)
    {
        StartCoroutine(NextLevelCoroutine(goal));
    }

    private IEnumerator NextLevelCoroutine(GameObject goal)
    {
        currentLevelIndex++;
        goal.TryGetComponent(out Animator goalAnimator);
        goalAnimator?.SetTrigger("Reached");
        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            yield return null;
        }
        // If next level is specified, load it
        if(nextLevelOverride >= 0)
            SceneManager.LoadScene(nextLevelOverride);
        // Otherwise just load the next level in order
        else
            SceneManager.LoadScene(currentLevelIndex);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(currentLevelIndex);
    }

}
