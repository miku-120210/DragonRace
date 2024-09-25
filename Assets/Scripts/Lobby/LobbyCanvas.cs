using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using FusionUtilsEvents;
using System.Threading.Tasks;


public class LobbyCanvas : MonoBehaviour
{
    private GameMode _gameMode;

    public string Nickname = "Player";
    public GameLauncher Launcher;

    public FusionEvent OnPlayerJoinedEvent;
    public FusionEvent OnPlayerLeftEvent;
    public FusionEvent OnShutdownEvent;
    public FusionEvent OnPlayerDataSpawnedEvent;


    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _lobbyToHomeButton;
    [SerializeField] private Button _singlePlayButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _startButton;

    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private GameObject _LobbyPanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private TMP_InputField _nickname;
    [SerializeField] private TMP_InputField _room;
    [SerializeField] private TextMeshProUGUI _lobbyPlayerText;
    [SerializeField] private TextMeshProUGUI _lobbyRoomName;


    void Start()
    {
        _homeButton.onClick.AddListener(OnClickHome);
        _lobbyToHomeButton.onClick.AddListener(LeaveLobby);

        _singlePlayButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Single);
        });
        _hostButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Host);
        });
        _joinButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Client);
        });

        _nextButton.onClick.AddListener(StartLauncherAsync);

        _startButton.onClick.AddListener(() =>
        {
            StartButton();
        });
    }

    private void OnEnable()
    {
        OnPlayerJoinedEvent.RegisterResponse(ShowLobbyCanvas);
        OnShutdownEvent.RegisterResponse(ResetCanvas);
        OnPlayerLeftEvent.RegisterResponse(UpdateLobbyList);
        OnPlayerDataSpawnedEvent.RegisterResponse(UpdateLobbyList);
    }

    private void OnDisable()
    {
        OnPlayerJoinedEvent.RemoveResponse(ShowLobbyCanvas);
        OnShutdownEvent.RemoveResponse(ResetCanvas);
        OnPlayerLeftEvent.RemoveResponse(UpdateLobbyList);
        OnPlayerDataSpawnedEvent.RemoveResponse(UpdateLobbyList);
    }


    public void SetGameMode(GameMode gameMode)
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Lobby);
        _gameMode = gameMode;
        _singlePlayButton.gameObject.SetActive(false);
        _hostButton.gameObject.SetActive(false);
        _joinButton.gameObject.SetActive(false);
        _inputPanel.gameObject.SetActive(true);
    }

    public async void StartLauncherAsync()
    {
        Launcher = FindObjectOfType<GameLauncher>();
        Nickname = _nickname.text;
        PlayerPrefs.SetString("Nickname", Nickname);
        PlayerPrefs.Save();

        _loadingPanel.gameObject.SetActive(true);

        await Launcher.Launch(_gameMode, _room.text);

        _loadingPanel.gameObject.SetActive(false);
        _inputPanel.SetActive(false);
    }


    public void StartButton()
    {
        FusionHelper.LocalRunner.SessionInfo.IsOpen = false;
        FusionHelper.LocalRunner.SessionInfo.IsVisible = false;
        LoadingManager.Instance.LoadRandomStage(FusionHelper.LocalRunner);
    }


    private void OnClickHome()
    {
        _singlePlayButton.gameObject.SetActive(true);
        _hostButton.gameObject.SetActive(true);
        _joinButton.gameObject.SetActive(true);
        _inputPanel.gameObject.SetActive(false);
        _LobbyPanel.gameObject.SetActive(false);
    }

    public void LeaveLobby()
    {
        _ = LeaveLobbyAsync();
    }
    private async Task LeaveLobbyAsync()
    {
        if (FusionHelper.LocalRunner.IsServer)
        {
            foreach (var player in FusionHelper.LocalRunner.ActivePlayers)
            {
                if (player != FusionHelper.LocalRunner.LocalPlayer)
                    FusionHelper.LocalRunner.Disconnect(player);
            }
        }
        await FusionHelper.LocalRunner?.Shutdown();
    }



    private void ResetCanvas(PlayerRef player, NetworkRunner runner)
    {
        OnClickHome();
        _startButton.gameObject.SetActive(runner.IsServer);
    }

    public void ShowLobbyCanvas(PlayerRef player, NetworkRunner runner)
    {
        _LobbyPanel.SetActive(true);
    }


    public void UpdateLobbyList(PlayerRef playerRef, NetworkRunner runner)
    {
        _startButton.gameObject.SetActive(runner.IsServer);
        string players = default;
        string isLocal;
        foreach (var player in runner.ActivePlayers)
        {
            isLocal = player == runner.LocalPlayer ? " (You)" : string.Empty;
            players += GameManager.Instance.GetPlayerData(player, runner)?.Nick + isLocal + " \n";
        }
        _lobbyPlayerText.text = players;
        Debug.Log("Player : " + players);
        _lobbyRoomName.text = $"Room: {runner.SessionInfo.Name}";
    }

}
