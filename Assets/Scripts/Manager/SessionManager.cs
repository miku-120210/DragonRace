using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using FusionUtilsEvents;
using System.Threading.Tasks;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public FusionEvent OnPlayerLeftEvent;
    public FusionEvent OnRunnerShutDownEvent;
    public FusionEvent OnPlayerJoinedEvent;
    [SerializeField] private int _maxPlayers = 1;

    private Dictionary<PlayerRef, PlayerData> _playerData = new ();


    public enum GameState
    {
        Lobby,
        Playing,
        Loading
    }

    private GameState State { get; set; }

    [Space]

    public LevelManager LoadLevelManager;

    [SerializeField] private GameObject _exitCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.transform.parent.gameObject);
        }
        DontDestroyOnLoad(transform.parent);
    }

    private void OnEnable()
    {
        OnPlayerLeftEvent.RegisterResponse(PlayerDisconnected);
        OnRunnerShutDownEvent.RegisterResponse(DisconnectedFromSession);
        //OnPlayerJoinedEvent.RegisterResponse(ValidatePlayerCount);
    }

    private void OnDisable()
    {
        OnPlayerLeftEvent.RemoveResponse(PlayerDisconnected);
        OnRunnerShutDownEvent.RemoveResponse(DisconnectedFromSession);
        //OnPlayerJoinedEvent.RemoveResponse(ValidatePlayerCount);
    }

    public void SetGameState(GameState state)
    {
        State = state;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && State == GameState.Playing)
        {
            _exitCanvas.SetActive(!_exitCanvas.activeInHierarchy);
        }
    }

    public PlayerData GetPlayerData(PlayerRef player, NetworkRunner runner)
    {
        NetworkObject NO;
        if (runner.TryGetPlayerObject(player, out NO))
        {
            PlayerData data = NO.GetComponent<PlayerData>();
            return data;
        }
        else
        {
            Debug.LogWarning("Player not found");
            return null;
        }
    }

    public void AllowAllPlayersInputs()
    {
        foreach (PlayerBehaviour behaviour in FindObjectsOfType<PlayerBehaviour>())
        {
            behaviour.SetInputsAllowed(true);
        }
    }

    ///// <summary>
    ///// Start player's spectator state.
    ///// </summary>
    public void SetPlayerSpectating(PlayerBehaviour playerBehaviour)
    {
        FindObjectOfType<CameraManager>().SetSpectating();
        playerBehaviour.SetInputsAllowed(false);
    }

    public void PlayerDisconnected(PlayerRef player, NetworkRunner runner)
    {
        runner.Despawn(_playerData[player].Instance);
        runner.Despawn(_playerData[player].Object);
        _playerData.Remove(player);
    }

    //private void aaa(PlayerRef player, NetworkRunner runner)
    //{
    //    _ = ValidatePlayerCount(player, runner);
    //}

    private void ValidatePlayerCount(PlayerRef player, NetworkRunner runner)
    {
        if (_playerData.Count > _maxPlayers)
        {
            LobbyCanvas.Instance.LeaveLobby();
            //Debug.Log("Room is full.");
            //if (FusionHelper.LocalRunner.IsServer)
            //{
            //    foreach (var a in FusionHelper.LocalRunner.ActivePlayers)
            //    {
            //        if (a != FusionHelper.LocalRunner.LocalPlayer)
            //            FusionHelper.LocalRunner.Disconnect(a);
            //    }
            //}
            //await FusionHelper.LocalRunner?.Shutdown();
        }
    }

    private async Task ShutdownRunner()
    {
        await FusionHelper.LocalRunner?.Shutdown();
        SetGameState(GameState.Lobby);
        _playerData.Clear();
    }

    private void DisconnectedFromSession(PlayerRef player, NetworkRunner runner)
    {
        Debug.Log("Disconnected from the session");
        ExitSession();
    }

    private void ExitSession()
    {
        _ = ShutdownRunner();
        LoadLevelManager.ResetLoadedScene();
        SceneManager.LoadScene(0);
        _exitCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        _ = ShutdownRunner();
        Application.Quit();
    }

    public void SetPlayerDataObject(PlayerRef objectInputAuthority, PlayerData playerData)
    {
        _playerData.Add(objectInputAuthority, playerData);
    }
}
