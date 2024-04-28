using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimelineButton : MonoBehaviour, ILeftMouseClick
{
    [SerializeField] private Button timelineButton;
    [SerializeField] private TextMeshProUGUI timelineButtonText;
    [SerializeField] private Image timelineButtonImage;

    public void SetData(Action buttonAction, [CanBeNull] string buttonText = null, [CanBeNull] Sprite buttonImage = null)
    {
        timelineButton.onClick.AddListener(() => buttonAction());
        timelineButtonText.text = buttonText;

        if (buttonImage != null)
            timelineButtonImage.sprite = buttonImage;
    }

    public void ClickMouse(RaycastHit hit)
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(hit.transform.gameObject, pointer, ExecuteEvents.submitHandler);
    }
}