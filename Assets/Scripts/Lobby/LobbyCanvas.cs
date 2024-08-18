using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
//using FusionUtilsEvents;
using System.Threading.Tasks;


public class LobbyCanvas : MonoBehaviour
{
    private GameMode _gameMode;

    public string Nickname = "Player";

    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _singlePlayButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _startButton;

    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private GameObject _LobbyPanel;

     void Start()
    {
        _homeButton.onClick.AddListener(OnClickHome);
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
        _startButton.onClick.AddListener(() =>
        {
            StartButton();
        });
    }

    public void SetGameMode(GameMode gameMode)
    {
        //GameManager.Instance.SetGameState(GameManager.GameState.Lobby);
        _gameMode = gameMode;
        _singlePlayButton.gameObject.SetActive(false);
        _hostButton.gameObject.SetActive(false);
        _joinButton.gameObject.SetActive(false);
        _inputPanel.gameObject.SetActive(true);
    }

    public void StartButton()
    {
        //FusionHelper.LocalRunner.SessionInfo.IsOpen = false;
        //FusionHelper.LocalRunner.SessionInfo.IsVisible = false;
        //LoadingManager.Instance.LoadNextLevel(FusionHelper.LocalRunner);
    }


    private void OnClickHome()
    {
        _singlePlayButton.gameObject.SetActive(true);
        _hostButton.gameObject.SetActive(true);
        _joinButton.gameObject.SetActive(true);
        _inputPanel.gameObject.SetActive(false);
        _LobbyPanel.gameObject.SetActive(false);
    }
}
