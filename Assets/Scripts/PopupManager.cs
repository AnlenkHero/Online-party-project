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

    
    private void Update()
    {
        if (currentChatBubbleInstance != null)
        {
            currentChatBubbleInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }
    

    public static void ShowPanelAboveObject(Transform parentTransform, Vector3 position, string info)
    {
        if (_instance.currentChatBubbleInstance != null)
        {
            HidePanel();
        }

        _instance.currentChatBubbleInstance =
            Instantiate(_instance.chatBubblePrefab, parentTransform);
        _instance.currentChatBubbleInstance.transform.localPosition = position;
        _instance.currentChatBubbleInstance.SetData(info);
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