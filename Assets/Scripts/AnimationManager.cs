using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public abstract class AnimationManager : MonoBehaviour, IInteractable
{
    public bool IsInteractable { get; set; } = true;
    public string Description => "Press E to play animation";
    [SerializeField] protected List<string> animationNames;
    [SerializeField] protected Animator animator;
    [SerializeField] protected PhotonView photonView;
    protected Queue<string> animationQueue = new();
    private bool IsAnimationPlaying;
    private bool _isPopUpShown;

    protected void Awake()
    {
        foreach (var name in animationNames)
        {
            AddAnimationToQueue(name);
        }
    }

    protected void AddAnimationToQueue(string animationName)
    {
        animationQueue.Enqueue(animationName);
    }

    public void OnAnimationEnd()
    {
        IsAnimationPlaying = false;
        if (animationQueue.Count > 0)
        {
            var nextAnimation = animationQueue.Dequeue();
            animator.SetTrigger(nextAnimation);
            IsAnimationPlaying = true;
        }
        else if (!IsAnimationPlaying)
        {
            CallbackAfterAllAnimations();
        }
    }


    public void Interact()
    {
        HideInfo();
        photonView.RPC(nameof(DefaultStartAnimationInteraction), RpcTarget.All);
        photonView.RPC(nameof(AnimationStartInteraction), RpcTarget.All);
    }

    public void ShowInfo()
    {
        if (_isPopUpShown) return;

        PopupManager.ShowPanelAboveObject(transform, Vector3.up, Description);
        _isPopUpShown = true;
    }

    public void HideInfo()
    {
        PopupManager.HidePanel();
        _isPopUpShown = false;
    }


    [PunRPC]
    public void DefaultStartAnimationInteraction()
    {
        if (animationQueue.Count > 0)
        {
            IsInteractable = false;
            IsAnimationPlaying = true;
            var nextAnimation = animationQueue.Dequeue();
            animator.SetTrigger(nextAnimation);
        }
    }

    [PunRPC]
    public abstract void AnimationStartInteraction();

    [PunRPC]
    protected virtual void CallbackAfterAllAnimations()
    {
        Debug.Log("OnAnimationEnd in AnimationManager called");
    }
}