using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

[System.Serializable]
public class VideoChoice
{
    public string videoChoiceName;
    public VideoClip videoClip;
    public List<NextVideoChoices> nextChoices;
    public List<VideoChoice> nextVideoChoices;
}

public class VideoTimelineController : MonoBehaviour, IInteractable
{
    [SerializeField] private PhotonView view;
    public VideoPlayer videoPlayer;
    public GameObject choicesCanvas;
    public TimelineButton choiceButtonPrefab;
    public Transform choicesPanel;
    public List<VideoChoice> videoChoices;
    private VideoChoice currentChoice;
    public bool IsInteractable { get; set; } = true;
    public string Description => "Press E to play the video";
    public static Action OnTimelineStarted;
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
        if (videoChoices.Count > 0)
        {
            SetInitialChoice();
            view.RPC(nameof(SetVideoClipAndPlay),RpcTarget.AllBufferedViaServer, videoChoices[0].videoClip.name);
            OnTimelineStarted?.Invoke();
        }

        view.RPC(nameof(StopInteraction), RpcTarget.AllBufferedViaServer);
    }
    
    private void SetInitialChoice()
    {
        currentChoice = videoChoices[0];
    }
    [PunRPC]
    private void StopInteraction()
    {
        IsInteractable = false;
    }


    void PlayVideoChoice(VideoChoice choice)
    {
        currentChoice = choice;
        string clipName = choice.videoClip.name;
        view.RPC(nameof(SetVideoClipAndPlay),RpcTarget.AllBufferedViaServer, clipName);
    }

    [PunRPC]
    private void SetVideoClipAndPlay(string videoClipName)
    {
        VideoClip clipToPlay = FindVideoClipByName(videoClipName);
        if (clipToPlay != null)
        {
            videoPlayer.clip = clipToPlay;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnMovieFinished;
        }
        else
        {
            Debug.LogError("Video clip not found with name: " + videoClipName);
        }
    }

    private VideoClip FindVideoClipByName(string videoClipName)
    {
        // Assuming you have a way to find or load the clip by its name
        // This could be a direct lookup, Resources.Load, or similar method
        return Resources.Load<VideoClip>("Videos/" + videoClipName);
    }

    void OnMovieFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnMovieFinished;

        if (currentChoice.nextChoices.Count > 0)
        {
            ShowNextChoices(); // Show choices for nextChoices
        }
        else if (currentChoice.nextVideoChoices.Count > 0)
        {
            ShowVideoChoices(); // Show choices for nextVideoChoices
        }
        else
        {
            EndGame();
        }
    }

    void ShowNextChoices()
    {
        ShowChoices(currentChoice.nextChoices, (index) =>
        {
            choicesCanvas.SetActive(false);
            string clipName = currentChoice.nextChoices[index].videoClip.name;
            view.RPC(nameof(SetVideoClipAndPlay),RpcTarget.AllBufferedViaServer, clipName);
            currentChoice.nextChoices.RemoveAt(index);
        }, currentChoice.nextChoices.ConvertAll(choice => choice.choiceName));
    }

    void ShowVideoChoices()
    {
        ShowChoices(currentChoice.nextVideoChoices, (index) =>
        {
            choicesCanvas.SetActive(false);
            PlayVideoChoice(currentChoice.nextVideoChoices[index]);
        }, currentChoice.nextVideoChoices.ConvertAll(choice => choice.videoChoiceName));
    }

    void ShowChoices<T>(List<T> choices, Action<int> callback, List<string> names)
    {
        ClearChoices();

        choicesCanvas.SetActive(true);

        for (int i = 0; i < choices.Count; i++)
        {
            int choiceIndex = i;
            TimelineButton button = Instantiate(choiceButtonPrefab, choicesPanel);
            button.gameObject.SetActive(true);
            button.SetData(() => callback(choiceIndex), names[i]);
        }
    }


    void ClearChoices()
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndGame()
    {
        Debug.Log("Game Ended");
        choicesCanvas.SetActive(false);
    }
}