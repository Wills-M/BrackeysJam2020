using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    #region Singleton

    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    private int currentLevelIndex = 0;

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
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(currentLevelIndex);
    }

}
