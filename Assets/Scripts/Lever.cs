using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{

    [SerializeField]
    private bool on;
    [SerializeField]
    private Sprite offSprite;
    [SerializeField]
    private Sprite onSprite;

    [SerializeField]
    private GameObject door;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (on)
            spriteRenderer.sprite = onSprite;
        else
            spriteRenderer.sprite = offSprite;
    }

    public void Flip()
    {
        on = !on;
        UpdateSprite();
        door.SetActive(!door.activeSelf);
    }

}
