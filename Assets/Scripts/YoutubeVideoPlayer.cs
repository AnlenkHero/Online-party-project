using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer.Components;

public class YoutubeVideoPlayer : MonoBehaviour, IInteractable
{
    [SerializeField] private InvidiousVideoPlayer invidiousVideoPlayer;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material videoDisplayMaterial;
    [SerializeField] private PhotonView view;
    [SerializeField] private PopUpHandler popUpHandler;
    [SerializeField] private VideoPlayerUIManager uiManager;
    private string _currentYoutubeVideoLink;

    private void Start()
    {
        uiManager.OnPlayVideoButtonPressed += s => view.RPC(nameof(PlayYoutubeVideoByLink), RpcTarget.All, s);
    }

    [PunRPC]
    public async void PlayYoutubeVideoByLink(string youtubeVideoLink)
    {
        _currentYoutubeVideoLink = youtubeVideoLink;
        string videoId = GetYoutubeVideoId(youtubeVideoLink);

        if (!string.IsNullOrEmpty(videoId))
        {
            uiManager.UpdateDebugInfo($"Playing YouTube video with ID: {videoId}");
            

            AdjustRenderTexture(youtubeVideoLink);

            invidiousVideoPlayer.VideoId = videoId;
            await invidiousVideoPlayer.PlayVideoAsync();

            // Sync the video start time across all users
            view.RPC(nameof(SyncVideoTime), RpcTarget.All, videoPlayer.time);
        }
        else
        {
            uiManager.UpdateDebugInfo("Error: Invalid YouTube video link.");
        }
    }

    public static string GetYoutubeVideoId(string youtubeUrl)
    {
        var regex = new Regex(
            @"(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed|bedded)?)\/|\S*?[?&]v=|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})");

        var match = regex.Match(youtubeUrl);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return string.Empty;
    }

    private void AdjustRenderTexture(string youtubeVideoLink)
    {
        int width = 1920;
        int height = 1080;

        if (youtubeVideoLink.Contains("/shorts/"))
        {
            width = 1800;
            height = 3200;
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
        }

        renderTexture = new RenderTexture(width, height, 0);
        videoPlayer.targetTexture = renderTexture;

        if (videoDisplayMaterial != null)
        {
            videoDisplayMaterial.SetTexture("_EmissionMap", renderTexture);
            videoDisplayMaterial.EnableKeyword("_EMISSION");
        }

        Debug.Log($"RenderTexture adjusted to: {renderTexture.width}x{renderTexture.height}");
    }

    [PunRPC]
    public void SyncVideoTime(double time)
    {
        videoPlayer.time = time;
    }

    public bool IsInteractable { get; set; } = true;
    public bool IsUiInteraction { get; set; } = true;
    public string Description => "Press E to open YouTube Player";

    public void Interact(PhotonView photonView)
    {
        ShowInfo();
        uiManager.ShowUIPanel();
    }

    public void ShowInfo()
    {
        popUpHandler.ShowPopUp(Description, transform);
    }

    public void HideInfo()
    {
        popUpHandler.HidePopUp();
    }
}
