using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _winnerNickname = new TextMeshProUGUI[3];
    [SerializeField] private Image[] _winnerImage = new Image[3];

    private Animator _anim;

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

    public void FadeOut()
    {
        _anim.Play("FadeOut");
    }
}
