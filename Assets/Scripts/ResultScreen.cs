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

    private void Awake()
    {
        _endButton.onClick.AddListener(BackToTitle);
    }

    public void SetWinner(string nick, Color color, int place)
    {
        _winnerNickname[place].text = nick;
        _winnerImage[place].color = color;

        _winnerImage[place].gameObject.SetActive(true);
        _winnerNickname[place].gameObject.SetActive(true);
    }
    public void FadeIn()
    {
        _anim.Play("FadeIn");
    }

    private void BackToTitle()
    {
        _anim.Play("FadeOut");
        GameManager.Instance.ExitGame();
    }
}
