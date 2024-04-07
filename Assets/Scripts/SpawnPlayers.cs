using System;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView photonView;
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    private GameObject _player;
    private bool asd;

    private void Awake()
    {
        ZXC.OnPlayerSpawned += () =>  SpawnPlayer(true);
    }

    void SpawnPlayer(bool n)
    {
        if(_player != null)
        {
            PhotonNetwork.Destroy(_player);
//            photonView.RPC(nameof(AddPlayerToList), RpcTarget.AllBufferedViaServer,_player);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerModelIndex", out var modelIndexObject))
        {
            int modelIndex = (int)modelIndexObject;
            if (n)
                modelIndex = 1;
            int randomSpawnPoint = Random.Range(0, spawnPoints.Length);

            if (modelIndex >= 0 && modelIndex < playerPrefabs.Length)
            {
              _player =  PhotonNetwork.Instantiate(playerPrefabs[modelIndex].name, spawnPoints[randomSpawnPoint].position, spawnPoints[randomSpawnPoint].rotation);
  //            PlayerList.Players.Add(_player);
            }
            else
            {
                Debug.LogError("Invalid model index selected.");
            }
        }
    }

    [PunRPC]
    private void AddPlayerToList(GameObject player)
    {
        PlayerList.Players.Add(player);
    }
    [PunRPC]
    private void RemovePlayerFromList(GameObject player)
    {
        PlayerList.Players.Remove(player);
    }
    public override void OnJoinedRoom()
    {
        SpawnPlayer(false);
    }
}