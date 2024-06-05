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


    public void SetData(Action buttonAction, [CanBeNull] string buttonText = null)
    {
        timelineButton.onClick.AddListener(() => buttonAction());
        timelineButtonText.text = buttonText;


    }

    public void ClickMouse(RaycastHit hit)
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(hit.transform.gameObject, pointer, ExecuteEvents.submitHandler);
    }
}