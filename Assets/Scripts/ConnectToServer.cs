using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton; 
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject chooseModelPanel;
    [SerializeField] private ModelInfo[] playerModels;
    [SerializeField] private List<ModelInfo> instantiatedPlayerModels;
    [SerializeField] private TextMeshProUGUI modelName;
    [SerializeField] private Transform modelsParent;
    private int currentModelIndex;

    private void Awake()
    {
        leftArrowButton.onClick.AddListener(OnClickLeftArrow);
        rightArrowButton.onClick.AddListener(OnClickRightArrow);
        joinRoomButton.onClick.AddListener(JoinRoom);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void HideModel(int modelIndex)
    {
        instantiatedPlayerModels[modelIndex].gameObject.SetActive(false);
    }
    public void ChooseModel(int modelIndex)
    {
        HideModel(currentModelIndex);
        Hashtable playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerCustomProperties["PlayerModelIndex"] = modelIndex;
        PlayerPrefs.SetInt("PlayerModelIndex", modelIndex);
        instantiatedPlayerModels[modelIndex].transform.rotation = instantiatedPlayerModels[currentModelIndex].transform.rotation;
        instantiatedPlayerModels[modelIndex].gameObject.SetActive(true);
        modelName.text = playerModels[modelIndex].GetComponent<ModelInfo>().modelName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
        currentModelIndex = modelIndex;
    }

    public override void OnConnectedToMaster()
    {
        loadingPanel.SetActive(false);
        chooseModelPanel.SetActive(true);
        InitializeModels();
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new Photon.Realtime.RoomOptions { MaxPlayers = 20 }, null);
        SceneManager.LoadScene("MainScene");
    }

    private void InitializeModels()
    {
        foreach (var model in playerModels)
        {
            var go = Instantiate(model,modelsParent);
            instantiatedPlayerModels.Add(go);
            go.gameObject.SetActive(false);
        }
        ChooseModel(PlayerPrefs.GetInt("PlayerModelIndex",0));
    }

    private void OnClickLeftArrow()
    {
        if (PlayerPrefs.GetInt("PlayerModelIndex", 0) == 0)
        {
            ChooseModel(playerModels.Length - 1);
        }
        else
        {
            ChooseModel(PlayerPrefs.GetInt("PlayerModelIndex", 0) - 1);
        }
    }
    
    private void OnClickRightArrow()
    {
        if (PlayerPrefs.GetInt("PlayerModelIndex", 0) == playerModels.Length - 1)
        {
            ChooseModel(0);
        }
        else
        {
            ChooseModel(PlayerPrefs.GetInt("PlayerModelIndex", 0) + 1);
        }
    }
    
}