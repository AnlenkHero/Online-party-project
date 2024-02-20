
using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new Photon.Realtime.RoomOptions {MaxPlayers = 20}, null);
        SceneManager.LoadScene("MainScene");
    }
    
}
