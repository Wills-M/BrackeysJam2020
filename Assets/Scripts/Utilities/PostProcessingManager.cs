using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
    
    private VolumeProfile profile;

    [SerializeField]
    private Image screenImage;
    [SerializeField]
    private Image rewindImage;

    [SerializeField]
    private float caIntensity;
    private ChromaticAberration chromaticAberration;

    [SerializeField]
    private float paniniIntensity;
    private PaniniProjection paniniProjection;
    private float startPanini;

    [SerializeField]
    private float inOutTime;

    private void Awake()
    {
        Volume volume = GetComponent<Volume>();
        profile = volume.profile;

        profile.TryGet(out chromaticAberration);
        profile.TryGet(out paniniProjection);

        startPanini = paniniProjection.distance.value;
    }

    private IEnumerator StartCoroutine()
    {
        screenImage.enabled = true;
        rewindImage.enabled = true;
        for (float t = 0; t < inOutTime; t += Time.deltaTime)
        {
            paniniProjection.distance.Override((t / inOutTime) * (paniniIntensity - startPanini) + startPanini);
            chromaticAberration.intensity.Override((t / inOutTime) * caIntensity);
            yield return null;
        }
        paniniProjection.distance.Override(paniniIntensity);
        chromaticAberration.intensity.Override(caIntensity);
    }

    private IEnumerator EndCoroutine()
    {

        for (float t = 0; t < inOutTime; t += Time.deltaTime)
        {
            paniniProjection.distance.Override((1f - (t / inOutTime)) * (paniniIntensity - startPanini) + startPanini);
            //chromaticAberration.intensity.Override((1f - (t / inOutTime)) * caIntensity);
            yield return null;
        }

        rewindImage.enabled = false;
        screenImage.enabled = false;
        paniniProjection.distance.Override(startPanini);
        chromaticAberration.intensity.Override(0f);
    }

    public void RewindStart()
    {
        StartCoroutine(StartCoroutine());
    }

    public void RewindEnd()
    {
        StartCoroutine(EndCoroutine());
    }

}
