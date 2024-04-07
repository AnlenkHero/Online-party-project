using System;
using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
    [SerializeField] private AnimationManager animationManager;

    public void AnimationEnded()
    {
        animationManager.OnAnimationEnd();
    }
}