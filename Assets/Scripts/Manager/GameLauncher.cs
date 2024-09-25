using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class GameLauncher : MonoBehaviour
{
    public GameObject LauncherPrefab;

    public async Task Launch(GameMode _gameMode, string _room)
    {
        FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
        if (launcher == null)
            launcher = Instantiate(LauncherPrefab).GetComponent<FusionLauncher>();

        LevelManager lm = FindObjectOfType<LevelManager>();
        lm.Launcher = launcher;

        launcher.Launch(_gameMode, _room, lm);
        await Task.Run(() => launcher.Launch(_gameMode, _room, lm));
        if (_gameMode == GameMode.Single) return;
        await Task.Delay(4000);
    }
}
