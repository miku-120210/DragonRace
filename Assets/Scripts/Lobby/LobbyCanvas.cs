using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using FusionUtilsEvents;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;

public class LobbyCanvas : MonoBehaviour
{
    public static LobbyCanvas Instance;
    private GameMode _gameMode;
    
    public string Nickname = "Player";
    public GameLauncher Launcher;
    
    [Space]
    [Header("Fusion Event")]
    public FusionEvent OnPlayerJoinedEvent;
    public FusionEvent OnPlayerLeftEvent;
    public FusionEvent OnShutdownEvent;
    public FusionEvent OnPlayerDataSpawnedEvent;

    [Space] 
    [Header("Button")]
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _lobbyToHomeButton;
    [SerializeField] private Button _randomButton;
    [SerializeField] private Button _singlePlayButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _startButton;

    [Space]
    [Header("Game Object")]
    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _fullPanel;
    [SerializeField] private GameObject[] _roomNameObjects;

    [Space]
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _countdown;
    [SerializeField] private TMP_InputField _privateNickname;
    [SerializeField] private TMP_InputField _room;
    [SerializeField] private TextMeshProUGUI _lobbyPlayerText;
    [SerializeField] private TextMeshProUGUI _lobbyRoomName;

    [Space]
    [Header("Audio & Animation")]
    [SerializeField] AudioSource _bgm;
    [SerializeField] AudioClip _buttonSe;

    [SerializeField] private Animator _anim;

    public static ShutdownReason _shutdownReason;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _bgm.Play();
        _anim.Play("Title");
        _room.onValueChanged.AddListener(OnInputFieldChanged);
        
        _homeButton.onClick.AddListener(()=> 
        {
            OnClickHome();
            _bgm.PlayOneShot(_buttonSe);
         });
        _playButton.onClick.AddListener(() => 
        {
            _title.SetActive(false);
            _mainMenu.SetActive(true);
            _bgm.PlayOneShot(_buttonSe);
        });
        _lobbyToHomeButton.onClick.AddListener(() =>
        {
            _shutdownReason = ShutdownReason.Ok;
            LeaveLobby();
            _bgm.PlayOneShot(_buttonSe);
        });
        _randomButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.AutoHostOrClient);
            _bgm.PlayOneShot(_buttonSe);
        });
        _singlePlayButton.onClick.AddListener(() =>
        {
            SetGameMode(GameMode.Single);
            _bgm.PlayOneShot(_buttonSe);
        });
        _hostButton.onClick.AddListener(() =>
        {
            _nextButton.interactable = false;
            SetGameMode(GameMode.Host);
            _bgm.PlayOneShot(_buttonSe);
        });
        _joinButton.onClick.AddListener(() =>
        {
            _nextButton.interactable = false;
            SetGameMode(GameMode.Client);
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

    private void OnInputFieldChanged(string text)
    {
        _nextButton.interactable = !string.IsNullOrEmpty(text);
    }
    
    private void SetGameMode(GameMode gameMode)
    {
        var roomStatus = gameMode == GameMode.AutoHostOrClient
            ? FusionLauncher.RoomStatus.Random
            : FusionLauncher.RoomStatus.Private;
        _nextButton.onClick.AddListener(()=> 
        {
            StartLauncherAsync(roomStatus);
            _bgm.PlayOneShot(_buttonSe);
        });

        SessionManager.Instance.SetGameState(SessionManager.GameState.Lobby);
        _gameMode = gameMode;
        _mainMenu.SetActive(false);
        _inputPanel.gameObject.SetActive(true);
        if (gameMode == GameMode.AutoHostOrClient || gameMode == GameMode.Single)
        {
            _room.gameObject.SetActive(false);
            ChangeLobbyLayout();
        }
    }

    private async void StartLauncherAsync(FusionLauncher.RoomStatus  roomStatus)
    {
        Launcher = FindObjectOfType<GameLauncher>();
        Nickname = _privateNickname.text;
        PlayerPrefs.SetString("Nickname", Nickname);
        PlayerPrefs.Save();

        _loadingPanel.gameObject.SetActive(true);
        
        await Launcher.Launch(_gameMode, _room.text, roomStatus);

        _loadingPanel.gameObject.SetActive(false);
        _inputPanel.SetActive(false);
    }


    private void StartButton()
    {
        FusionHelper.LocalRunner.SessionInfo.IsOpen = false;
        FusionHelper.LocalRunner.SessionInfo.IsVisible = false;
        LoadingManager.Instance.LoadRandomStage(FusionHelper.LocalRunner);
        _bgm.Stop();
    }


    private void OnClickHome()
    {
        _title.SetActive(true);
        _homeButton.gameObject.SetActive(true);
        _mainMenu.SetActive(false);
        _room.gameObject.SetActive(true);
        _inputPanel.SetActive(false);
        _nextButton.interactable = true;
        _room.text = "";
        _lobbyPanel.SetActive(false);
        _fullPanel.SetActive(false);
    }
    
    private void LeaveLobby()
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
        _lobbyPanel.SetActive(true);
        _homeButton.gameObject.SetActive(false);
    }

    private void ChangeLobbyLayout()
    {
        foreach (var obj in _roomNameObjects) obj.SetActive(false);
        _lobbyPlayerText.fontSize = 30;
    }

    private void UpdateLobbyList(PlayerRef playerRef, NetworkRunner runner)
    {
        var active = _gameMode == GameMode.Single || (runner.IsServer && runner.SessionInfo.MaxPlayers == runner.ActivePlayers.Count());
        _startButton.gameObject.SetActive(active);
        string players = default;
        string isLocal;
        foreach (var player in runner.ActivePlayers)
        {
            isLocal = player == runner.LocalPlayer ? " (You)" : string.Empty;
            players += SessionManager.Instance.GetPlayerData(player, runner)?.Nick + isLocal + " \n";
        }
        _lobbyPlayerText.text = players;
        _lobbyRoomName.text = $"ROOM: {runner.SessionInfo.Name}";
    }
}
