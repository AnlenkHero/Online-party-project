using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine.Serialization;

[System.Serializable]
public class VideoChoice
{
    public string videoChoiceName;
    public VideoClip videoClip;
    public Sprite buttonImage;
    public bool isTransition;
    [FormerlySerializedAs("nextChoices")] public List<RequiredVideoChoice> nextRequiredChoices;
    public List<VideoChoice> nextVideoChoices;
}

public class VideoTimelineController : MonoBehaviour, IInteractable
{
    [SerializeField] private PhotonView view;
    public VideoPlayer videoPlayer;
    public GameObject choicesCanvas;
    public TimelineButton choiceButtonPrefab;
    public Transform choicesPanel;

    [FormerlySerializedAs("currentChoiceName")]
    public TextMeshProUGUI currentChoiceNameTMP;

    public Image currentChoiceImage;
    public List<VideoChoice> videoChoices;
    private VideoChoice _currentChoice;
    private string _currentChoiceName;
    private Sprite _currentChoiceSprite;
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
                videoChoices[0].videoChoiceName, videoChoices[0].buttonImage.name);
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
        view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.All, clipName, choice.videoChoiceName, choice.buttonImage.name);
    }

    [PunRPC]
    private void SetVideoClipAndPlay(string videoClipName, string choiceName, string choiceImage)
    {
        VideoClip clipToPlay = FindVideoClipByName(videoClipName);
        if (clipToPlay != null)
        {
            view.RPC(nameof(SetCurrentChoiceNameAndImage), RpcTarget.All, choiceName, choiceImage);
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
    
    private Sprite FindSpriteByName(string spriteName)
    {
        return Resources.Load<Sprite>("Sprites/" + spriteName);
    }

    [PunRPC]
    private void ShowCurrentChoice()
    {
        choicesCanvas.SetActive(true);
        currentChoiceNameTMP.text = _currentChoiceName;
        currentChoiceImage.sprite = _currentChoiceSprite;
    }

    [PunRPC]
    private void SetCurrentChoiceNameAndImage(string choiceName, string choiceImage)
    {
        choicesCanvas.SetActive(false);
        _currentChoiceName = choiceName;
        _currentChoiceSprite = FindSpriteByName(choiceImage);
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
                    _currentChoice.nextRequiredChoices[index].choiceName, _currentChoice.nextRequiredChoices[index].buttonImage.name);
                _currentChoice.nextRequiredChoices.RemoveAt(index);
            }, _currentChoice.nextRequiredChoices.ConvertAll(choice => choice.choiceName),
            _currentChoice.nextRequiredChoices.ConvertAll(choice => choice.buttonImage));
    }

    void ShowVideoChoices()
    {
        ShowChoices(_currentChoice.nextVideoChoices, (index) =>
            {
                choicesCanvas.SetActive(false);
                PlayVideoChoice(_currentChoice.nextVideoChoices[index]);
                if(!_currentChoice.nextVideoChoices[index].isTransition)
                    _currentChoice.nextVideoChoices.RemoveAt(index);
            }, _currentChoice.nextVideoChoices.ConvertAll(choice => choice.videoChoiceName),
            _currentChoice.nextVideoChoices.ConvertAll(choice => choice.buttonImage));
    }

    void ShowChoices<T>(List<T> choices, Action<int> callback, List<string> names, List<Sprite> buttonImages)
    {
        ClearChoices();

        choicesCanvas.SetActive(true);

        for (int i = 0; i < choices.Count; i++)
        {
            int choiceIndex = i;
            TimelineButton button = Instantiate(choiceButtonPrefab, choicesPanel);
            button.gameObject.SetActive(true);
            button.SetData(() => callback(choiceIndex), names[i], buttonImages[i]);
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