using TMPro;
using UnityEngine;

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bubbleText;

    public void SetData(string info)
    {
        bubbleText.text = info;
    }
}