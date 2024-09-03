using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using Photon.Pun;
using TMPro;
using XNode;

public class VideoTimelineController : MonoBehaviour, IInteractable
{
    [SerializeField] private PhotonView view;
    public VideoPlayer videoPlayer;
    public GameObject choicesCanvas;
    public TimelineButton choiceButtonPrefab;
    public Transform choicesPanel;

    public TextMeshProUGUI currentChoiceNameTMP;

    public VideoChoiceNode initialChoiceNode;
    private VideoChoiceNode _currentChoiceNode;
    private string _currentChoiceName;

    private HashSet<Node> usedNodes = new HashSet<Node>();

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
        if (initialChoiceNode != null)
        {
            SetInitialChoice();
            view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.All, initialChoiceNode.videoClip.name, initialChoiceNode.videoChoiceName);
        }

        view.RPC(nameof(StopInteraction), RpcTarget.AllBufferedViaServer);
    }

    private void SetInitialChoice()
    {
        _currentChoiceNode = initialChoiceNode;
    }

    [PunRPC]
    private void StopInteraction()
    {
        IsInteractable = false;
    }

    void PlayVideoChoice(VideoChoiceNode choiceNode)
    {
        if (choiceNode.isTransition)
            _currentChoiceNode = choiceNode;
        string clipName = choiceNode.videoClip.name;
        view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.All, clipName, choiceNode.videoChoiceName);

        // Mark node as used
        if (!choiceNode.isTransition)
        {
            MarkNodeAsUsed(choiceNode);
        }
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

        List<RequiredVideoChoiceNode> requiredChoices = GetConnectedNodes<RequiredVideoChoiceNode>(_currentChoiceNode, "nextRequiredChoices");
        List<VideoChoiceNode> videoChoices = GetConnectedNodes<VideoChoiceNode>(_currentChoiceNode, "nextVideoChoices");

        requiredChoices = FilterUsedNodes(requiredChoices);
        videoChoices = FilterUsedNodes(videoChoices);

        bool hasRequiredChoices = requiredChoices.Count > 0;
        bool hasVideoChoices = videoChoices.Count > 0;

        if (hasRequiredChoices)
        {
            ShowNextChoices(requiredChoices);
        }
        else if (hasVideoChoices)
        {
            ShowVideoChoices(videoChoices);
        }
        else
        {
            EndGame();
        }
    }

    void ShowNextChoices(List<RequiredVideoChoiceNode> choices)
    {
        ShowChoices(choices, (index) =>
        {
            choicesCanvas.SetActive(false);
            string clipName = choices[index].videoClip.name;
            view.RPC(nameof(SetVideoClipAndPlay), RpcTarget.AllBufferedViaServer, clipName, choices[index].choiceName);

            // Mark node as used
            MarkNodeAsUsed(choices[index]);
        }, choices.ConvertAll(choice => choice.choiceName));
    }

    void ShowVideoChoices(List<VideoChoiceNode> choices)
    {
        if (choices.Count == 1)
        {
            PlayVideoChoice(choices[0]);
        }
        else
        {
            ShowChoices(choices, (index) =>
            {
                choicesCanvas.SetActive(false);
                PlayVideoChoice(choices[index]);

                // Mark node as used
                if (!choices[index].isTransition)
                {
                    MarkNodeAsUsed(choices[index]);
                }
            }, choices.ConvertAll(choice => choice.videoChoiceName));
        }
    }

    void ShowChoices<T>(List<T> choices, Action<int> callback, List<string> names) where T : Node
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

    private List<T> GetConnectedNodes<T>(Node node, string fieldName) where T : Node
    {
        NodePort port = node.GetPort(fieldName);
        List<T> connectedNodes = new List<T>();

        if (port != null)
        {
            for (int i = 0; i < port.ConnectionCount; i++)
            {
                NodePort connectedPort = port.GetConnection(i);
                T connectedNode = connectedPort.node as T;
                if (connectedNode != null)
                {
                    connectedNodes.Add(connectedNode);
                }
            }
        }

        return connectedNodes;
    }

    private void MarkNodeAsUsed(Node node)
    {
        usedNodes.Add(node);
    }

    private List<T> FilterUsedNodes<T>(List<T> nodes) where T : Node
    {
        return nodes.Where(node => !usedNodes.Contains(node)).ToList();
    }
}
