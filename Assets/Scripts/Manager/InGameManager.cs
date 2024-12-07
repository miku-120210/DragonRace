using UnityEngine;
using Fusion;
using System.Linq;
using FusionUtilsEvents;
using DG.Tweening;
using TMPro;

public class InGameManager : NetworkBehaviour
{
  private const float StartTime = 4;
  public FusionEvent OnPlayerDisconnectEvent;
  [SerializeField] private float _levelTime = 10f;
  [SerializeField] private float _previewTime = 3f;
  [SerializeField] private float _endPos;

  [Networked] private TickTimer PreviewTimer { get; set; }
  [Networked] private TickTimer StartTimer { get; set; }
  [Networked] private TickTimer Timer { get; set; }

  [SerializeField] private TextMeshProUGUI _startTimer;
  [SerializeField] private TextMeshProUGUI _timer;
  [SerializeField] private Camera _playerCamera;
  [SerializeField] private Camera _previewCamera;
  [SerializeField] private int _playersAlreadyFinish = 0;
  private bool _isPreviewActive = true;
  private bool _isStartActive = false;

  [Networked, Capacity(3)]
  private NetworkArray<PlayerRef> _winners => default;
  private NetworkArray<PlayerRef> Winners { get => _winners; }

  [SerializeField] private ResultScreen _resultScreen;

  [SerializeField] AudioSource _audioSource;
  [SerializeField] AudioClip _startSe;

  public override void Spawned()
  {
    FindObjectOfType<PlayerSpawnManager>().SpawnPlayer(Runner);
    StartStagePreview();
    _playerCamera.enabled = false;
  }
  void OnEnable()
  {
    if (FusionHelper.LocalRunner.IsServer)
    {
      OnPlayerDisconnectEvent.RegisterResponse(CheckWinnersOnPlayerDisconnect);
    }
  }

  void OnDisable()
  {
    if (FusionHelper.LocalRunner.IsServer)
    {
      OnPlayerDisconnectEvent.RemoveResponse(CheckWinnersOnPlayerDisconnect);
    }
  }
  
  public override void FixedUpdateNetwork()
  {
    // プレビュー終了時
    if (PreviewTimer.Expired(Runner) && _isPreviewActive)
    {
      _isPreviewActive = false;
      RPC_StartLevel();
    }
    // スタートカウントダウン終了時
    if (StartTimer.Expired(Runner) && _isStartActive)
    {
      Timer = TickTimer.CreateFromSeconds(Runner, _levelTime);
      SessionManager.Instance.AllowAllPlayersInputs();
      _isStartActive = false;
      RPC_PlayBGM();
    }
    // ゲームカウントダウン
    if (Timer.IsRunning)
    {
      if (Object.HasStateAuthority && Timer.Expired(Runner) && (_playersAlreadyFinish < 3 || _playersAlreadyFinish < Runner.ActivePlayers.Count()))
      {
        RPC_FinishLevel();
        Timer = TickTimer.None;
      }
    }
  }
    public override void Render()
    {
        if (StartTimer.IsRunning && _startTimer.gameObject.activeInHierarchy)
        {
          _startTimer.text = (int?)StartTimer.RemainingTime(Runner) == 0 ? "GO!" : ((int?)StartTimer.RemainingTime(Runner)).ToString();
        }

        if (StartTimer.Expired(Runner))
        {
            _startTimer.gameObject.SetActive(false);
            _timer.gameObject.SetActive(true);
        }

        if (Timer.IsRunning)
        {
            _timer.text = ((int?)Timer.RemainingTime(Runner)).ToString();
        }
    }

    private void StartStagePreview()
    {
      PreviewTimer = TickTimer.CreateFromSeconds(Runner, _previewTime);
      _previewCamera.gameObject.transform.DOLocalMoveY(_endPos,_previewTime - 1f).SetDelay(0.5f);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_StartLevel()
  {
    _audioSource.PlayOneShot(_startSe);    
    _previewCamera.enabled = false;
    _playerCamera.enabled = true;
    _isStartActive = true;
    SetLevelStartValues();
    if(Runner.IsClient) return;
    SessionManager.Instance.SetGameState(SessionManager.GameState.Playing);
  }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_PlayBGM()
    {
      _audioSource.Play();
    }

    private void SetLevelStartValues()
  {
    _playersAlreadyFinish = 0;
    StartTimer = TickTimer.CreateFromSeconds(Runner, StartTime);
    _startTimer.gameObject.SetActive(true);
    for (int i = 0; i < 3; i++)
    {
      Winners.Set(i, PlayerRef.None);
    }
  }
  
  private void CheckWinnersOnPlayerDisconnect(PlayerRef player, NetworkRunner runner)
  {
    Debug.Log(runner.ActivePlayers.Count());
    if (_playersAlreadyFinish >= 3 || _playersAlreadyFinish >= runner.ActivePlayers.Count())
    {
      RPC_FinishLevel();
    }
  }

    public void PlayerOnFinishLine(PlayerRef player, PlayerBehaviour playerBehaviour)
    {
        if (_playersAlreadyFinish >= 3 || _winners.Contains(player)) { return; }

        _winners.Set(_playersAlreadyFinish, player);

        _playersAlreadyFinish++;

        playerBehaviour.SetInputsAllowed(false);

        if (_playersAlreadyFinish >= 3 || _playersAlreadyFinish >= Runner.ActivePlayers.Count())
        {
            RPC_FinishLevel();
        }
    }
    
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
  private void RPC_FinishLevel()
  {
    _audioSource.Stop();
    int i = 0;
    foreach (var player in Winners)
    {
      PlayerData data = SessionManager.Instance.GetPlayerData(player, Runner);
      if (data != null)
      {
        _resultScreen.SetWinner(data.Nick.ToString(), data.Instance.GetComponent<PlayerBehaviour>().PlayerColor, i);
      }
      i++;
    }
    _resultScreen.gameObject.SetActive(true);
    _resultScreen.FadeIn();
  }

}
