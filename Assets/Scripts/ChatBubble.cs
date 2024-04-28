using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private Image icon;

    public void SetData(string info, [CanBeNull] Sprite chatBubbleImage = null)
    {
        bubbleText.text = info;
        if (chatBubbleImage)
            icon.sprite = chatBubbleImage;
    }
}