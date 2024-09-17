using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class VideoTimeSlider : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private PhotonView photonView;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Slider timeSlider;

    public event System.Action<double> OnTimeUpdated;
    public event System.Action<double> OnSetTotalTime;

    private bool isDragging = false;  // Track whether the user is currently dragging the slider

    private void Awake()
    {
        // Set min/max values for the slider
        timeSlider.minValue = 0f;
        timeSlider.maxValue = 1f;  // The slider's max value represents the normalized time (0.0 - 1.0)

        // Add a listener for slider value changes
        timeSlider.onValueChanged.AddListener(OnSliderValueChangedLocal);
    }

    private void Update()
    {
        // Only update the slider if not dragging and the video is playing
        if (!isDragging && videoPlayer.isPlaying && videoPlayer.length > 0)
        {
            // Calculate and set the slider value based on video time
            timeSlider.value = (float)(videoPlayer.time / videoPlayer.length);
            OnTimeUpdated?.Invoke(videoPlayer.time);
        }
    }

    // Called when the slider value changes (user drags the slider)
    private void OnSliderValueChangedLocal(float value)
    {
        if (isDragging  && videoPlayer.length > 0)
        {
            // Calculate the new video time based on the slider value and update the video locally
            double newTime = value * videoPlayer.length;
            videoPlayer.time = newTime;

            // We do not broadcast the RPC here during dragging to avoid conflicting updates
        }
    }

    // Called when the user starts dragging the slider
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;  // Set dragging flag
        videoPlayer.Pause();  // Pause the video while dragging
    }

    // Called when the user ends dragging the slider
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;  // Reset dragging flag
        videoPlayer.Play();  // Resume the video

        // Once dragging ends, sync the final video time with all other players
        photonView.RPC(nameof(SetTimeForAll), RpcTarget.All, videoPlayer.time);
    }

    // Called via RPC to update the time for all players
    [PunRPC]
    public void SetTimeForAll(double newTime)
    {
        // Only update if the user is not currently dragging to avoid conflicts
        if (!isDragging && videoPlayer.length > 0)
        {
            videoPlayer.time = newTime;
            timeSlider.value = (float)(newTime / videoPlayer.length);  // Update the slider for all users
        }
    }

    // Called via RPC to set the total video time
    [PunRPC]
    public void SetTotalTime(double totalTime)
    {
        OnSetTotalTime?.Invoke(totalTime);
    }

    // Called via RPC to reset the video time
    [PunRPC]
    public void ResetTime()
    {
        timeSlider.value = 0;
        OnTimeUpdated?.Invoke(0);
    }
}
