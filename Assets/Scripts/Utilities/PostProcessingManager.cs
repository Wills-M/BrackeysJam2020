using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
    
    private VolumeProfile profile;


    [SerializeField]
    private float caIntensity;
    private ChromaticAberration chromaticAberration;

    [SerializeField]
    private float inOutTime;

    private void Awake()
    {
        Volume volume = GetComponent<Volume>();
        profile = volume.profile;

        profile.TryGet(out chromaticAberration);
    }

    private IEnumerator StartCoroutine()
    {
        for (float t = 0; t < inOutTime; t += Time.deltaTime)
        {
            chromaticAberration.intensity.Override((t / inOutTime) * caIntensity);
            yield return null;
        }
        chromaticAberration.intensity.Override(caIntensity);
    }

    private IEnumerator EndCoroutine()
    {

        for (float t = 0; t < inOutTime; t += Time.deltaTime)
        {
            chromaticAberration.intensity.Override((1f - (t / inOutTime)) * caIntensity);
            yield return null;
        }
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
