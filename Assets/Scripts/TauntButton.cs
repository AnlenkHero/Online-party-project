using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TauntButton : MonoBehaviour
{
    [SerializeField] private Button tauntButton;
    [SerializeField] private TextMeshProUGUI tauntName;

    public void SetData(string animationName, Action onClick)
    {
        this.tauntName.text = animationName;
        tauntButton.onClick.AddListener(() => onClick());
    }
}
