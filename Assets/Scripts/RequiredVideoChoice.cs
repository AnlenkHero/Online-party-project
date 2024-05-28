using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class RequiredVideoChoice
{
    public string choiceName;
    public VideoClip videoClip;
    public Sprite buttonImage;
    public bool isTransition;
}