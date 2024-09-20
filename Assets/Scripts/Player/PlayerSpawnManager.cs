using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public NetworkPrefabRef Player;
    [SerializeField] private List<Transform> _spawnPoints;
    public static List<Vector2> PlayerSpawnPoints = new();

    private void Awake()
    {
        for (var i = 0; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i].position = PlayerSpawnPoints[i];
        }
    }
    public void SpawnPlayer(NetworkRunner runner, string nick = "")
    {
        if (!runner.IsClient)
        {
            foreach (var activePlayer in runner.ActivePlayers)
            {
                if (!runner.IsServer) return;
                
                NetworkObject playerObj = runner.Spawn(Player, PlayerSpawnPoints[1], Quaternion.identity, activePlayer, InitializeObjBeforeSpawn);
                PlayerData data = GameManager.Instance.GetPlayerData(activePlayer, runner);
                data.Instance = playerObj;

                playerObj.GetComponent<PlayerBehaviour>().Nickname = data.Nick;
            }
        }
    }
    private void InitializeObjBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        var behaviour = obj.GetComponent<PlayerBehaviour>();
        behaviour.PlayerColor = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), 255);
    }

}
