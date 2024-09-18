using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public NetworkPrefabRef Player;
    [SerializeField] private List<GameObject> _spawnPoints;
    public static List<Vector2> PlayerSpawnPoints = new();

    private void Awake()
    {
        for (var i = 0; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i].transform.position = PlayerSpawnPoints[i];
        }
    }
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player, string nick = "")
    {
        if (runner.IsServer)
        {
            NetworkObject playerObj = runner.Spawn(Player, PlayerSpawnPoints[1], Quaternion.identity, player, InitializeObjBeforeSpawn);

            PlayerData data = GameManager.Instance.GetPlayerData(player, runner);
            data.Instance = playerObj;

            playerObj.GetComponent<PlayerBehavior>().Nickname = data.Nick;
        }
    }
    private void InitializeObjBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        var behaviour = obj.GetComponent<PlayerBehavior>();
        behaviour.PlayerColor = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), 255);
    }

}
