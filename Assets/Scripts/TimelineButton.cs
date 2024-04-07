using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineButton : MonoBehaviour
{
  [SerializeField] private Button timelineButton;
  [SerializeField] private TextMeshProUGUI timelineButtonText;

  public void SetData(Action buttonAction, string buttonText)
  {
    timelineButton.onClick.AddListener(() => buttonAction());
    timelineButtonText.text = buttonText;
  }
}