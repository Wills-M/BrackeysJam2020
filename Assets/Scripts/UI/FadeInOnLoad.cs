using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FadeEffect fade = FindObjectOfType<FadeEffect>();
        if (fade)
            fade.FadeIn();
        else
            Debug.LogWarning(name + " | Could not find FadeEffect");
    }
}
