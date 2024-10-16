using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class GameLauncher : MonoBehaviour
{
    public GameObject LauncherPrefab;

    public async Task Launch(GameMode _gameMode, string _room, FusionLauncher.RoomStatus roomStatus)
    {
        FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
        if (launcher == null)
            launcher = Instantiate(LauncherPrefab).GetComponent<FusionLauncher>();
        
        launcher.SetRoomStatus(roomStatus, "");

        LevelManager lm = FindObjectOfType<LevelManager>();
        lm.Launcher = launcher;

        launcher.Launch(_gameMode, _room, lm);
        await Task.Run(() => launcher.Launch(_gameMode, _room, lm));
        if (_gameMode == GameMode.Single) return;
        await Task.Delay(4000);
    }
}
