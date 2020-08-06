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

}
