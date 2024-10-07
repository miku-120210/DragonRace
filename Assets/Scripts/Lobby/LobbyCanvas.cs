using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using FusionUtilsEvents;
using System.Threading.Tasks;
using UnityEngine.Serialization;
using System.Linq;

public class LobbyCanvas : MonoBehaviour
{
    public static LobbyCanvas Instance;
    private GameMode _gameMode;

    [SerializeField] private int _maxPlayers = 2;

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
    [SerializeField] private Button _backButton;

    [SerializeField] private GameObject _title;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject _fullPanel;
    [SerializeField] private TMP_InputField _nickname;
    [SerializeField] private TMP_InputField _room;
    [SerializeField] private TextMeshProUGUI _lobbyPlayerText;
    [SerializeField] private TextMeshProUGUI _lobbyRoomName;

    [SerializeField] AudioSource _bgm;
    [SerializeField] AudioClip _buttonSe;

    [SerializeField] private Animator _anim;

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
        _backButton.onClick.AddListener(() =>
        {
            OnClickHome();
            _bgm.PlayOneShot(_buttonSe);
        });
    }

    private void OnEnable()
    {
        OnPlayerJoinedEvent.RegisterResponse(ShowLobbyCanvas);
        OnShutdownEvent.RegisterResponse(ResetCanvas);
        OnPlayerLeftEvent.RegisterResponse(UpdateLobbyList);
        OnPlayerDataSpawnedEvent.RegisterResponse(UpdateLobbyList);
        OnPlayerDataSpawnedEvent.RegisterResponse(ValidatePlayerCount);
    }

    private void OnDisable()
    {
        OnPlayerJoinedEvent.RemoveResponse(ShowLobbyCanvas);
        OnShutdownEvent.RemoveResponse(ResetCanvas);
        OnPlayerLeftEvent.RemoveResponse(UpdateLobbyList);
        OnPlayerDataSpawnedEvent.RemoveResponse(UpdateLobbyList);
    }


    private void SetGameMode(GameMode gameMode)
    {
        SessionManager.Instance.SetGameState(SessionManager.GameState.Lobby);
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


    private void StartButton()
    {
        FusionHelper.LocalRunner.SessionInfo.IsOpen = false;
        FusionHelper.LocalRunner.SessionInfo.IsVisible = false;
        LoadingManager.Instance.LoadRandomStage(FusionHelper.LocalRunner);
        _bgm.Stop();
    }


    private void OnClickHome()
    {
        _singlePlayButton.gameObject.SetActive(true);
        _hostButton.gameObject.SetActive(true);
        _joinButton.gameObject.SetActive(true);
        _inputPanel.gameObject.SetActive(false);
        _lobbyPanel.gameObject.SetActive(false);
    }

    private void ValidatePlayerCount(PlayerRef player, NetworkRunner runner)
    {
        var players = runner.ActivePlayers;

        if (players == null) return;

        int playerCount = players.Count();
        Debug.Log($"現在のプレイヤー人数: {playerCount}");

        if (playerCount > _maxPlayers)
        {
            if (runner.LocalPlayer == player)
            {
                FusionHelper.LocalRunner.Disconnect(player);
                Debug.Log("player ID" + player.PlayerId);
                ShowRoomFullMessage();
                //　追い出す
            }
            UpdateLobbyList(player, runner);
        }
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
        _lobbyPanel.SetActive(true);
        _homeButton.gameObject.SetActive(false);
    }


    private void UpdateLobbyList(PlayerRef playerRef, NetworkRunner runner)
    {
        _startButton.gameObject.SetActive(runner.IsServer);
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

    public void ShowRoomFullMessage()
    {
        _fullPanel.SetActive(true);
        _lobbyPanel.SetActive(false);
    }
}
