using UnityEngine;
using Fusion;

public class FusionLauncher : MonoBehaviour
{
    private const int MaxPlayers = 4;
    private NetworkRunner _runner;
    private ConnectionStatus _status;
    private RoomStatus _roomStatus;

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Failed,
        Connected,
        Loading,
        Loaded
    }
    
    public enum RoomStatus
    {
        Random,
        Private
    }

    public async void Launch(GameMode mode, string room,
        INetworkSceneManager sceneLoader)
    {
        SetConnectionStatus(ConnectionStatus.Connecting, "");

        DontDestroyOnLoad(gameObject);

        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.name = name;
        _runner.ProvideInput = mode != GameMode.Server;

        var roomIdentify = _roomStatus + "_" + room;
        Debug.Log("RoomName :" + room);
        
        var startGameResult = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = room,
            SceneManager = sceneLoader,
            PlayerCount = MaxPlayers
        });
    }

    public void SetConnectionStatus(ConnectionStatus status, string message)
    {
        _status = status;
    }

    public void SetRoomStatus(RoomStatus roomStatus, string message)
    {
        _roomStatus = roomStatus;
    }
}
