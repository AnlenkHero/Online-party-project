using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class ChairInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private PhotonView photonView;
    public bool IsInteractable { get; set; } = true;
    public string Description { get; private set; } = "Press E to sit on chair";
    public bool _isSitting;
    [SerializeField] private CinemachineVirtualCamera tvCamera;
    private bool _isPopUpShown;
    private PhotonView callerView;
    [SerializeField] private Sprite icon;

    public void ShowInfo()
    {
        if (_isPopUpShown) return;

        PopupManager.ShowPanelAboveObject(transform, Vector3.up, Description, icon);
        _isPopUpShown = true;
    }

    public void HideInfo()
    {
        PopupManager.HidePanel();
        _isPopUpShown = false;
    }

    public void Interact(PhotonView view)
    {
        HideInfo();
        callerView = view;
        if (!_isSitting)
        {
            callerView.gameObject.GetComponent<ThirdPersonController>().SetupSit(transform.position);
            Description = "Press E to stand up";
            _isSitting = true;
            HideInfo();
            tvCamera.gameObject.SetActive(true);
        }
        else
        {
            callerView.gameObject.GetComponent<ThirdPersonController>().StandUp();
            Description = "Press E to sit on chair";
            _isSitting = false;
            HideInfo();
            tvCamera.gameObject.SetActive(false);
        }
    }
}