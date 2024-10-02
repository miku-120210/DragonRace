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
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _lobbyToHomeButton;
    [SerializeField] private Button _singlePlayButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _startButton;

    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private GameObject _LobbyPanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private TMP_InputField _nickname;
    [SerializeField] private TMP_InputField _room;
    [SerializeField] private TextMeshProUGUI _lobbyPlayerText;
    [SerializeField] private TextMeshProUGUI _lobbyRoomName;

    [SerializeField] AudioSource _bgm;
    [SerializeField] AudioClip _buttonSe;

    [SerializeField] private Animator _anim;


    void Start()
    {
        _bgm.Play();
        _anim.Play("Title");

        _homeButton.onClick.AddListener(()=> 
        {
            OnClickHome();
            _bgm.PlayOneShot(_buttonSe);
         });
        _playButton.onClick.AddListener(() => 
        {
            _title.SetActive(false);
            _mainMenu.SetActive(true);
        });
        _lobbyToHomeButton.onClick.AddListener(() =>
        {
            LeaveLobby();
            _bgm.PlayOneShot(_buttonSe);
        });

        _singlePlayButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Single);
            _bgm.PlayOneShot(_buttonSe);
        });
        _hostButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Host);
            _bgm.PlayOneShot(_buttonSe);
        });
        _joinButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Client);
            _bgm.PlayOneShot(_buttonSe);
        });

        _nextButton.onClick.AddListener(()=> 
        {
            StartLauncherAsync();
            _bgm.PlayOneShot(_buttonSe);
        });

        _startButton.onClick.AddListener(() =>
        {
            StartButton();
            _bgm.PlayOneShot(_buttonSe);
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

    private async void StartLauncherAsync()
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

    private void ShowLobbyCanvas(PlayerRef player, NetworkRunner runner)
    {
        _loadingPanel.SetActive(false);
        _LobbyPanel.SetActive(true);
        _homeButton.gameObject.SetActive(false);
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
        _lobbyRoomName.text = $"ROOM: {runner.SessionInfo.Name}";
    }

}
