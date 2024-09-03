using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using XNode;

public class VideoChoiceNode : Node
{
    [Input] public VideoChoiceNode previousChoice;
    [Output] public List<RequiredVideoChoiceNode> nextRequiredChoices;
    [Output] public List<VideoChoiceNode> nextVideoChoices;

    public string videoChoiceName;
    public VideoClip videoClip;
    public bool isTransition;
}

public class RequiredVideoChoiceNode : Node
{
    [Input] public VideoChoiceNode previousChoice;

    public string choiceName;
    public VideoClip videoClip;
    public bool isTransition;
}