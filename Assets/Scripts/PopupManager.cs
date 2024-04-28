using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private static PopupManager _instance;
    [SerializeField] private ChatBubble chatBubblePrefab;
    private ChatBubble currentChatBubbleInstance;

    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    private void LateUpdate()
    {
        if (currentChatBubbleInstance != null)
        {
            var rotation = Camera.main.transform.rotation;
            currentChatBubbleInstance.transform.LookAt(
                currentChatBubbleInstance.transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        }
    }
    

    public static void ShowPanelAboveObject(Transform parentTransform, Vector3 position, string info, [CanBeNull] Sprite chatBubbleImage = null)
    {
        if (_instance.currentChatBubbleInstance != null)
        {
            HidePanel();
        }

        _instance.currentChatBubbleInstance =
            Instantiate(_instance.chatBubblePrefab, parentTransform);
        _instance.currentChatBubbleInstance.transform.localPosition = position;
        _instance.currentChatBubbleInstance.SetData(info, chatBubbleImage);
    }

    public static void HidePanel()
    {
        if (_instance.currentChatBubbleInstance != null)
        {
            Destroy(_instance.currentChatBubbleInstance.gameObject);
            _instance.currentChatBubbleInstance = null;
        }
    }
    
}