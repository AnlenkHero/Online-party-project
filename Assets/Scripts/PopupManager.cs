using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private static PopupManager _instance;

    [SerializeField] private GameObject panelPrefab;

    private GameObject currentPanelInstance;
    private PhotonView _photonView;
    public static event Action OnPlayerSpawned;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
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
        if (Input.GetKeyDown(KeyCode.K))
        {
            _photonView.RPC(nameof(SpawnPlayer), RpcTarget.All);
        }

        if (currentPanelInstance != null)
        {
            currentPanelInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    [PunRPC]
    private void SpawnPlayer()
    {
        OnPlayerSpawned?.Invoke();
    }

    public static void ShowPanelAboveObject(Transform parentTransform, Vector3 position)
    {
        if (_instance.currentPanelInstance != null)
        {
            HidePanel();
        }

        _instance.currentPanelInstance =
            Instantiate(_instance.panelPrefab, parentTransform);
        _instance.currentPanelInstance.transform.localPosition = position;
    }

    public static void HidePanel()
    {
        if (_instance.currentPanelInstance != null)
        {
            Destroy(_instance.currentPanelInstance);
            _instance.currentPanelInstance = null;
        }
    }
}