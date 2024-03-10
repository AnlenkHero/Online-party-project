using System;
using Photon.Pun;
using UnityEngine;

public class AnimationManager : MonoBehaviour, IInteractable
{
    public string Description => "Press E to play animation";
    public static event Action OnPlayerSpawned;
    private bool _isPopUpShown;
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void Interact()
    {
        HideInfo();
        _photonView.RPC(nameof(SpawnPlayer), RpcTarget.All);
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
    private void SpawnPlayer()
    {
        OnPlayerSpawned?.Invoke();
    }
}