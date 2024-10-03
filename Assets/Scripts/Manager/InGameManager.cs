using UnityEngine;
using Fusion;
using System.Linq;
using FusionUtilsEvents;
using TMPro;

public class InGameManager : NetworkBehaviour
{
  public FusionEvent OnPlayerDisconnectEvent;
  [SerializeField] private float _levelTime = 10f;

  [Networked] private TickTimer StartTimer { get; set; }
  [Networked] private TickTimer Timer { get; set; }

  [SerializeField] private TextMeshProUGUI _startTimer;
  [SerializeField] private TextMeshProUGUI _timer;
  [SerializeField]
  private int _playersAlreadyFinish = 0;
  private bool _isInitializedTimer = false;

  [Networked, Capacity(3)]
  private NetworkArray<PlayerRef> _winners => default;
  private NetworkArray<PlayerRef> Winners { get => _winners; }

  [SerializeField] private ResultScreen _resultScreen;

  [SerializeField] AudioSource _audioSource;
  [SerializeField] AudioClip _startSe;

  public override void Spawned()
  {
    FindObjectOfType<PlayerSpawnManager>().SpawnPlayer(Runner);
    StartLevel();
    _audioSource.PlayOneShot(_startSe);
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
    if (StartTimer.Expired(Runner) && !_isInitializedTimer)
    {
      Timer = TickTimer.CreateFromSeconds(Runner, _levelTime);
      SessionManager.Instance.AllowAllPlayersInputs();
      _isInitializedTimer = true;
      _audioSource.Play();
    }

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


    public void StartLevel()
  {
    SetLevelStartValues();
    // StartLevelMusic();
    LoadingManager.Instance.FinishLoadingScreen();
    SessionManager.Instance.SetGameState(SessionManager.GameState.Playing);
  }
  private void SetLevelStartValues()
  {
    _playersAlreadyFinish = 0;
    StartTimer = TickTimer.CreateFromSeconds(Runner, 4);
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
