using TMPro;
using UnityEngine;

public class NicknameText : MonoBehaviour
{
  private TextMeshProUGUI _nicknameText;

  public void SetupNickname(string name)
  {
    if (_nicknameText == null) _nicknameText = GetComponent<TextMeshProUGUI>();
    _nicknameText.text = name;
  }
}
