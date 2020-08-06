﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
class AnimComponent : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        if(!TryGetComponent(out animator)) {
            // bad thing
        }
    }

    public enum AnimID { Moving, Falling, Pushing }
    public void SetAnimation(AnimID animID, bool active)
    {
        switch(animID) {
            case AnimID.Moving:
                animator.SetBool("Moving", active);
                break;
        }
    }
}
