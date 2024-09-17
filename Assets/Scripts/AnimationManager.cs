using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class AnimationManager : MonoBehaviour, IInteractable
{
    [SerializeField] private PopUpHandler popUpHandler;
    public bool IsInteractable { get; set; } = true;
    public bool IsUiInteraction { get; set; }
    public string Description => "Press E to revive players";
    [SerializeField] protected List<string> animationNames;
    [SerializeField] protected Animator animator;
    [SerializeField] protected PhotonView photonView;
    [SerializeField] protected Sprite icon;
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


    public void Interact(PhotonView view)
    {
        HideInfo();
        photonView.RPC(nameof(DefaultStartAnimationInteraction), RpcTarget.All);
        photonView.RPC(nameof(AnimationStartInteraction), RpcTarget.All);
    }

    public void ShowInfo()
    {
        popUpHandler.ShowPopUp(Description, transform);
    }

    public void HideInfo()
    {
        popUpHandler.HidePopUp();
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