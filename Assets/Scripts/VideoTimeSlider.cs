using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class VideoTimeSlider : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public PhotonView photonView;  // Ensure this is assigned in the Inspector
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Slider timeSlider;

    public event System.Action<double> OnTimeUpdated;
    public event System.Action<double> OnSetTotalTime;

    private bool isDragging = false;
    private bool ignoreNetworkUpdate = false;

    private void Awake()
    {
        // Set slider min and max values
        timeSlider.minValue = 0f;
        timeSlider.maxValue = 1f;

        // Add listener for local slider changes
        timeSlider.onValueChanged.AddListener(OnSliderValueChangedLocal);
    }

    private void Update()
    {
        // Only update the slider if not dragging and video is playing
        if (!isDragging && videoPlayer.isPlaying)
        {
            // Update the slider value based on the video time
            float normalizedTime = 0f;
            if (videoPlayer.length > 0)
                normalizedTime = (float)(videoPlayer.time / videoPlayer.length);

            // Only update if the slider value is different to avoid unnecessary updates
            if (!Mathf.Approximately(timeSlider.value, normalizedTime))
            {
                ignoreNetworkUpdate = true;  // Prevent reacting to this change in OnValueChanged
                timeSlider.value = normalizedTime;
                ignoreNetworkUpdate = false;
            }

            // Update the current time display
            OnTimeUpdated?.Invoke(videoPlayer.time);
        }
    }

    private void OnSliderValueChangedLocal(float value)
    {
        if (ignoreNetworkUpdate)
            return;  // Ignore updates caused by network synchronization

        if (isDragging)
        {
            // Update the video time locally
            double newTime = value * videoPlayer.length;
            videoPlayer.time = newTime;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        videoPlayer.Pause();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        videoPlayer.Play();

        // After dragging, synchronize the new time with all players
        double newTime = videoPlayer.time;
        photonView.RPC(nameof(SyncVideoTime), RpcTarget.OthersBuffered, newTime);
    }

    [PunRPC]
    private void SyncVideoTime(double newTime, PhotonMessageInfo info)
    {
        // If we're currently dragging, ignore incoming network updates
        if (isDragging)
            return;

        // Update the video time and slider value
        videoPlayer.time = newTime;
        if (videoPlayer.length > 0)
        {
            ignoreNetworkUpdate = true;  // Prevent reacting to this change in OnValueChanged
            timeSlider.value = (float)(newTime / videoPlayer.length);
            ignoreNetworkUpdate = false;
        }

        // Update the current time display
        OnTimeUpdated?.Invoke(newTime);
    }

    [PunRPC]
    public void SetTotalTime(double totalTime)
    {
        OnSetTotalTime?.Invoke(totalTime);
    }

    [PunRPC]
    public void ResetTime()
    {
        timeSlider.value = 0;
        OnTimeUpdated?.Invoke(0);
    }
}
