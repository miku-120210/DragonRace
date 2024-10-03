using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _winnerNickname = new TextMeshProUGUI[3];
    [SerializeField] private Image[] _winnerImage = new Image[3];

    [SerializeField] private Animator _anim;
    [SerializeField] private Button _endButton;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _resultSe;
    [SerializeField] private AudioClip _buttonSe;

    private void Awake()
    {
        _endButton.onClick.AddListener(BackToTitle);
    }

    public void SetWinner(string nick, Color color, int place)
    {
        _winnerImage[place].gameObject.SetActive(true);
        _winnerNickname[place].gameObject.SetActive(true);

        _winnerNickname[place].text = nick;
        _winnerImage[place].color = color;
    }
    public void FadeIn()
    {
        _anim.Play("FadeIn");
    }

    private void BackToTitle()
    {
        _audioSource.PlayOneShot(_buttonSe);
        _anim.Play("FadeOut");
        SessionManager.Instance.ExitGame();
    }

    private void ResultSE()
    {
        _audioSource.PlayOneShot(_resultSe);
    }
}
