using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class LevelManager : NetworkSceneManagerDefault
{
    [HideInInspector]
    public FusionLauncher Launcher;
    [SerializeField] private LoadingManager _loadingManager;
    private Scene _loadedScene;

    public void ResetLoadedScene()
    {
        _loadingManager.ResetLastLevelsIndex();
        _loadedScene = default;
    }

    protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
    {
        _loadingManager.StartLoadingScreen(_loadingManager._lastLevelIndex);
        SessionManager.Instance.SetGameState(SessionManager.GameState.Loading);
        Launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loading, "");
        yield return new WaitForSeconds(5.5f);
        yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
        Launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loaded, "");
        yield return new WaitForSeconds(0f);
        _loadingManager.FinishLoadingScreen(_loadingManager._lastLevelIndex);

    }
}
