﻿using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    /// <summary>
    /// Graphic to fade in/out from (ideally a black screen)
    /// </summary>
    [SerializeField] private Graphic fadePrefab;

    /// <summary>
    /// Instance of fadePrefab
    /// </summary>
    private Graphic fadeInstance;

    /// <summary>
    /// Duration of fade effect
    /// </summary>
    [SerializeField]
    private float duration = 1f;

    public bool fadeInProgress => fadeCoroutine != null;

    /// <summary>
    /// Fades from black screen to transparent
    /// </summary>
    public void FadeIn()
    {
        fadeCoroutine = StartCoroutine(Fade(1, 0));
    }

    /// <summary>
    /// Fades from transparent to black screen
    /// </summary>
    public void FadeOut()
    {
        fadeCoroutine = StartCoroutine(Fade(0, 1));
    }

    private Coroutine fadeCoroutine;

    /// <summary>
    /// Fades a black screen's alpha value from start to target
    /// </summary>
    /// <param name="start">Starting value</param>
    /// <param name="target">Target value</param>
    /// <returns></returns>
    private IEnumerator Fade(float start, float target)
    {
        // Instantiate if haven't already
        if (!fadeInstance)
            fadeInstance = Instantiate(fadePrefab, canvas.transform);

        // Fade color from start to target
        SetAlpha(fadeInstance, start);
        float t = 0;
        while (t < 1)
        {
            float alpha = Mathf.Lerp(start, target, t);
            SetAlpha(fadeInstance, alpha);

            t += Time.deltaTime / duration;
            yield return null;
        }

        // Ensure we hit target
        SetAlpha(fadeInstance, target);

        fadeCoroutine = null;
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }
}
