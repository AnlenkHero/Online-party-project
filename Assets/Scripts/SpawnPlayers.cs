using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;

    void SpawnPlayer()
    {
        object modelIndexObject;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PlayerModelIndex", out modelIndexObject))
        {
            int modelIndex = (int)modelIndexObject;
            int randomSpawnPoint = Random.Range(0, spawnPoints.Length);

            if (modelIndex >= 0 && modelIndex < playerPrefabs.Length)
            {
                PhotonNetwork.Instantiate(playerPrefabs[modelIndex].name, spawnPoints[randomSpawnPoint].position, spawnPoints[randomSpawnPoint].rotation);
            }
            else
            {
                Debug.LogError("Invalid model index selected.");
            }
        }
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }
}