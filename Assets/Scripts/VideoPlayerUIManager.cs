using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerUIManager : MonoBehaviour
{
    [SerializeField] private PhotonView view;
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private TMP_InputField videoLinkInputField;
    [SerializeField] private TextMeshProUGUI debugInfoText;
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoTimeSlider videoTimeSlider;

    public event Action<string> OnPlayVideoButtonPressed;

    private void Awake()
    {
        closeButton.onClick.AddListener(EndUIInteraction);
        playButton.onClick.AddListener(OnPlayButtonClicked);

        videoTimeSlider.OnTimeUpdated += d => view.RPC(nameof(UpdateCurrentTimeText), RpcTarget.All, d);
        videoTimeSlider.OnSetTotalTime += d => view.RPC(nameof(UpdateTotalTimeText), RpcTarget.All, d);

        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    [PunRPC]
    private void OnPlayButtonClicked()
    {
        string youtubeLink = videoLinkInputField.text;

        if (string.IsNullOrEmpty(youtubeLink))
        {
            UpdateDebugInfo("Error: YouTube video link is empty.");
        }
        else
        {
            OnPlayVideoButtonPressed?.Invoke(youtubeLink);
        }
    }

    private void OnVideoStarted(VideoPlayer vp)
    {
        if (videoPlayer.length > 0)
        {
            view.RPC(nameof(SetTotalTimeForAll), RpcTarget.All, videoPlayer.length);
        }
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        view.RPC(nameof(ResetVideoTime), RpcTarget.All);
    }

    [PunRPC]
    private void SetTotalTimeForAll(double totalTime)
    {
        if (totalTime > 0)
        {
            videoTimeSlider.SetTotalTime(totalTime);
        }
    }

    [PunRPC]
    private void ResetVideoTime()
    {
        videoTimeSlider.ResetTime();
    }

    [PunRPC]
    private void UpdateCurrentTimeText(double currentTime)
    {
        currentTimeText.text = FormatTime(currentTime);
    }

    [PunRPC]
    private void UpdateTotalTimeText(double totalTime)
    {
        totalTimeText.text = FormatTime(totalTime);
    }

    private string FormatTime(double time)
    {
        int minutes = Mathf.FloorToInt((float)time / 60);
        int seconds = Mathf.FloorToInt((float)time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateDebugInfo(string message)
    {
        if (debugInfoText != null)
        {
            debugInfoText.text = message;
        }
    }

    private void EndUIInteraction()
    {
        HideUIPanel();
        UIEventManager.RaiseUIInteractionEnded();
    }

    public void HideUIPanel()
    {
        UIPanel.SetActive(false);
    }

    public void ShowUIPanel()
    {
        UIPanel.SetActive(true);
    }
}
