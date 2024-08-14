using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Threading.Tasks;


public class LobbyCanvas : MonoBehaviour
{
    private GameMode _gameMode;

    public string Nickname = "Player";

    [SerializeField] private Button _singlePlayButton;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;

     void Start()
    {
        _singlePlayButton.onClick.AddListener(() =>
        {
            Debug.Log("SinglePlay");
        });
        _hostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host");
        });
        _joinButton.onClick.AddListener(() =>
        {
            Debug.Log("Join");
        });

    }

}
