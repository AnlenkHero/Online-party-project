using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void SpawnPlayer()
    {
        int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[randomSpawnPoint].position,
            spawnPoints[randomSpawnPoint].rotation);
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }
}