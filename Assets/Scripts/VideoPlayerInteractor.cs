using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;


public class VideoPlayerInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private PhotonView photonView;
    [SerializeField] private VideoPlayer videoPlayer;
    public string Description => "Press E to play the video";
    private bool _isPopUpShown;

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

    public void Interact()
    {
        HideInfo();
        photonView.RPC(nameof(PlayVideo), RpcTarget.All);
    }

    [PunRPC]
    private void PlayVideo()
    {
        videoPlayer.Play();
    }
}