using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

[System.Serializable]
public class VideoChoice : MonoBehaviour
{
    public string videoChoiceName;
    public VideoClip videoClip;
    public bool isTransition;
    [FormerlySerializedAs("nextChoices")] public List<RequiredVideoChoice> nextRequiredChoices;
    public List<VideoChoice> nextVideoChoices;
}