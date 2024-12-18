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

    [SerializeField] private int _length = 6;

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
    }

    private void OnDisable()
    {
        OnPlayerLeftEvent.RemoveResponse(PlayerDisconnected);
        OnRunnerShutDownEvent.RemoveResponse(DisconnectedFromSession);
    }

    public void SetGameState(GameState state)
    {
        State = state;
    }

    private void Update()
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
    
    private async Task ShutdownRunner()
    {
        await FusionHelper.LocalRunner?.Shutdown();
        SetGameState(GameState.Lobby);
        _playerData.Clear();
    }

    private void DisconnectedFromSession(PlayerRef player, NetworkRunner runner)
    {
        ExitSession();
    }

    public void ExitSession()
    {
        _ = ShutdownRunner();
        LoadLevelManager.ResetLoadedScene();
        if (LobbyCanvas._shutdownReason == ShutdownReason.GameIsFull) return;
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

    public string GenerateRandomRoomString()
    {
        var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] room = new char[_length];
        var random = new System.Random();
        for (int i = 0; i < _length; i++)
        {
            room[i] = characters[random.Next(characters.Length)];
        }
        return new string(room);
    }
}
