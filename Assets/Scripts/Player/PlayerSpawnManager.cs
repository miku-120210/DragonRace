using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef Player;
    [SerializeField] private List<Transform> _spawnPoints;
    public static List<Vector2> PlayerSpawnPoints = new();

    private void Awake()
    {
        for (var i = 0; i < _spawnPoints.Count; i++)
        {
            //_spawnPoints[i].position = PlayerSpawnPoints[i];
        }
    }
    public void SpawnPlayer(NetworkRunner runner, string nick = "")
    {
        if (!runner.IsClient)
        {
            foreach (var activePlayer in runner.ActivePlayers)
            {
                if (!runner.IsServer) return;
                var playerNum = activePlayer.PlayerId - 1;
                NetworkObject playerObj = runner.Spawn(Player, _spawnPoints[playerNum].position, Quaternion.identity, activePlayer, InitializeObjBeforeSpawn);
                PlayerData data = SessionManager.Instance.GetPlayerData(activePlayer, runner);
                data.Instance = playerObj;

                playerObj.GetComponent<PlayerBehaviour>().Nickname = data.Nick;
                Debug.Log("Player Number: " + data.Instance.GetComponent<PlayerBehaviour>().PlayerID);
            }
        }
    }
    private void InitializeObjBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        var behaviour = obj.GetComponent<PlayerBehaviour>();
        behaviour.PlayerColor = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), 255);
    }

    #region UnusedCallbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    #endregion

}
