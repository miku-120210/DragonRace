using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;
using System.Threading.Tasks;
using System;
using FusionUtilsEvents;
using TMPro;
using UnityEngine.Serialization;

public class InGameManager : NetworkBehaviour
{
  public FusionEvent OnPlayerDisconnectEvent;
  [SerializeField] private float _levelTime = 300f;

  [Networked] private TickTimer StartTimer { get; set; }
  [Networked] private TickTimer Timer { get; set; }

  [SerializeField] private TextMeshProUGUI _startTimer;
  [SerializeField] private TextMeshProUGUI _timer;
  [SerializeField]
  private int _playersAlreadyFinish = 0;

  [Networked, Capacity(3)]
  private NetworkArray<PlayerRef> _winners => default;
  public NetworkArray<PlayerRef> Winners { get => _winners; }

  // private FinishRaceScreen _finishRace;
  
  public override void Spawned()
  {
    FindObjectOfType<PlayerSpawnManager>().SpawnPlayer(Runner);
    // _finishRace = FindObjectOfType<FinishRaceScreen>();
    StartLevel();
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
    if (StartTimer.Expired(Runner))
    {
      Timer = TickTimer.CreateFromSeconds(Runner, _levelTime);
      GameManager.Instance.AllowAllPlayersInputs();
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

  public void StartLevel()
  {
    SetLevelStartValues();
    // StartLevelMusic();
    LoadingManager.Instance.FinishLoadingScreen();
    GameManager.Instance.SetGameState(GameManager.GameState.Playing);
  }
  private void SetLevelStartValues()
  {
    _playersAlreadyFinish = 0;
    StartTimer = TickTimer.CreateFromSeconds(Runner, 5);
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

  [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
  private void RPC_FinishLevel()
  {
    int i = 0;
    foreach (var player in Winners)
    {
      PlayerData data = GameManager.Instance.GetPlayerData(player, Runner);
      if (data != null)
      {
        Debug.Log("Player Number: "+data.Instance.GetComponent<PlayerBehaviour>().PlayerID);
        // _finishRace.SetWinner(data.Nick.ToString(), data.Instance.GetComponent<PlayerBehaviour>().PlayerColor, i);
      }
      i++;
    }

    // _finishRace.FadeIn();
    //
    // _finishRace.Invoke("FadeOut", 5f);

    // kokode back to home
  }

}
