using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;


public class VideoPlayerInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private PopUpHandler popUpHandler;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private VideoPlayer videoPlayer;
    public bool IsInteractable { get; set; } = true;
    public bool IsUiInteraction { get; set; }
    public string Description => "Press E to play the video";

    private bool _isPopUpShown;

    public void ShowInfo()
    {
        popUpHandler.ShowPopUp(Description, transform);
    }

    public void HideInfo()
    {
        popUpHandler.HidePopUp();
    }

    public void Interact(PhotonView view)
    {
        HideInfo();
        photonView.RPC(nameof(PlayVideo), RpcTarget.All);
        photonView.RPC(nameof(StopInteraction), RpcTarget.AllBufferedViaServer);
    }

    [PunRPC]
    private void StopInteraction()
    {
        IsInteractable = false;
    }
    [PunRPC]
    private void PlayVideo()
    {
        videoPlayer.Play();
    }
}