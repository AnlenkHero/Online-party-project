using System;
using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    private GameObject _player;
    private bool asd;

    private void Awake()
    {
        ThirdPersonController.OnPlayerSpawned += () =>  SpawnPlayer(true);
    }

    void SpawnPlayer(bool n)
    {
        if(_player != null)
        {
            PhotonNetwork.Destroy(_player);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerModelIndex", out var modelIndexObject))
        {
            int modelIndex = (int)modelIndexObject;
            if (n)
                modelIndex = 0;
            int randomSpawnPoint = Random.Range(0, spawnPoints.Length);

            if (modelIndex >= 0 && modelIndex < playerPrefabs.Length)
            {
              _player =  PhotonNetwork.Instantiate(playerPrefabs[modelIndex].name, spawnPoints[randomSpawnPoint].position, spawnPoints[randomSpawnPoint].rotation);
            }
            else
            {
                Debug.LogError("Invalid model index selected.");
            }
        }
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer(false);
    }
}