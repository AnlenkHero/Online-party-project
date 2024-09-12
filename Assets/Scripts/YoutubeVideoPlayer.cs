using System;
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
    [SerializeField] private string _currentYoutubeVideoLink;
    [SerializeField] private PopUpHandler popUpHandler;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayYoutubeVideoByLink(_currentYoutubeVideoLink);
        }
    }

    [PunRPC]
    public async void PlayYoutubeVideoByLink(string youtubeVideoLink)
    {
        _currentYoutubeVideoLink = youtubeVideoLink;
        string videoId = GetYoutubeVideoId(youtubeVideoLink);

        if (!string.IsNullOrEmpty(videoId))
        {
            AdjustRenderTexture(youtubeVideoLink);

            invidiousVideoPlayer.VideoId = videoId;
            await invidiousVideoPlayer.PlayVideoAsync();
        }
        else
        {
            Debug.LogError("Invalid YouTube video link.");
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

    public bool IsInteractable { get; set; } = true;
    public string Description  => "Press E to play Youtube Video";

    public void Interact(PhotonView photonView)
    {
        HideInfo();
        view.RPC(nameof(PlayYoutubeVideoByLink), RpcTarget.All, _currentYoutubeVideoLink);
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