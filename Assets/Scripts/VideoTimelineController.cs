using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine.Serialization;



public class VideoTimelineController : MonoBehaviour, IInteractable
{
 
    [SerializeField] private PhotonView view;
    public VideoPlayer videoPlayer;
    public GameObject choicesCanvas;
    public TimelineButton choiceButtonPrefab;
    public Transform choicesPanel;

    [FormerlySerializedAs("currentChoiceName")]
    public TextMeshProUGUI currentChoiceNameTMP;
    

    public List<VideoChoice> videoChoices;
    private VideoChoice _currentChoice;
    private string _currentChoiceName;

    public bool IsInteractable { get; set; } = true;
    public string Description => "Press E to play the video";
    private bool _isPopUpShown;
    [SerializeField] private Vector3 popUpOffset;
    [SerializeField] private Sprite icon;

    public void ShowInfo()
    {
        if (_isPopUpShown) return;

        PopupManager.ShowPanelAboveObject(transform, popUpOffset, Description, icon);
        _isPopUpShown = true;
    }

    public void HideInfo()
    {
        PopupManager.HidePanel();
        _isPopUpShown = false;
    }

    public void Interact(PhotonView photonView)
    {
        HideInfo();
        if (videoChoices.Count > 0)
        {
            SetInitialChoice();
            view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.All, videoChoices[0].videoClip.name,
                videoChoices[0].videoChoiceName);
        }

        view.RPC(nameof(StopInteraction), RpcTarget.AllBufferedViaServer);
    }

    private void SetInitialChoice()
    {
        _currentChoice = videoChoices[0];
    }

    [PunRPC]
    private void StopInteraction()
    {
        IsInteractable = false;
    }


    void PlayVideoChoice(VideoChoice choice)
    {
        if(choice.isTransition)
         _currentChoice = choice;
        string clipName = choice.videoClip.name;
        view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.All, clipName, choice.videoChoiceName);
    }

    [PunRPC]
    private void SetVideoClipAndPlay(string videoClipName, string choiceName)
    {
        VideoClip clipToPlay = FindVideoClipByName(videoClipName);
        if (clipToPlay != null)
        {
            view.RPC(nameof(SetCurrentChoiceNameAndImage), RpcTarget.All, choiceName);
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
        return Resources.Load<VideoClip>("Videos/" + videoClipName);
    }
    
  

    [PunRPC]
    private void ShowCurrentChoice()
    {
        choicesCanvas.SetActive(true);
        currentChoiceNameTMP.text = _currentChoiceName;
    }

    [PunRPC]
    private void SetCurrentChoiceNameAndImage(string choiceName)
    {
        choicesCanvas.SetActive(false);
        _currentChoiceName = choiceName;
    }

    void OnMovieFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnMovieFinished;
        view.RPC(nameof(ShowCurrentChoice), RpcTarget.All);

        if (_currentChoice.nextRequiredChoices.Count > 0 && _currentChoice.nextRequiredChoices.Any(choice => choice.isTransition))
        {
            ShowNextChoices();
        }
        else if (_currentChoice.nextVideoChoices.Count > 0 && _currentChoice.nextVideoChoices.Any(choice => choice.isTransition))
        {
            ShowVideoChoices();
        }
        else
        {
            EndGame();
        }
    }


    void ShowNextChoices()
    {
        ShowChoices(_currentChoice.nextRequiredChoices, (index) =>
            {
                choicesCanvas.SetActive(false);
                string clipName = _currentChoice.nextRequiredChoices[index].videoClip.name;
                view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.AllBufferedViaServer, clipName,
                    _currentChoice.nextRequiredChoices[index].choiceName);
                _currentChoice.nextRequiredChoices.RemoveAt(index);
            }, _currentChoice.nextRequiredChoices.ConvertAll(choice => choice.choiceName));
    }

    void ShowVideoChoices()
    {
        if (_currentChoice.nextVideoChoices.Count == 1)
        {
            PlayVideoChoice(_currentChoice.nextVideoChoices[0]);
        }
        else
        {
            ShowChoices(_currentChoice.nextVideoChoices, (index) =>
            {
                choicesCanvas.SetActive(false);
                PlayVideoChoice(_currentChoice.nextVideoChoices[index]);
                if(!_currentChoice.nextVideoChoices[index].isTransition)
                    _currentChoice.nextVideoChoices.RemoveAt(index);
            }, _currentChoice.nextVideoChoices.ConvertAll(choice => choice.videoChoiceName));
        }
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