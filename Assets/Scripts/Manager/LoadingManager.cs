using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using Fusion.Sockets;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField] private Animator _loadingScreenAnimator;

    public int _lastLevelIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(transform.parent);
    }

    public void ResetLastLevelsIndex()
    {
        _lastLevelIndex = 0;
    }


    public void LoadRandomStage(NetworkRunner runner)
    {
        int sceneIndex = Random.Range(1, SceneManager.sceneCountInBuildSettings);
        if (_lastLevelIndex == sceneIndex)
        {
            sceneIndex = sceneIndex + 1 >= SceneManager.sceneCountInBuildSettings ? sceneIndex - 1 : sceneIndex + 1;
        }
        _lastLevelIndex = sceneIndex;
        string scenePath = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
        runner.LoadScene(scenePath);
    }

    public void StartLoadingScreen(int sceneIndex)
    {
        var anim = sceneIndex == 1 ? "StageA" : sceneIndex == 2 ? "StageB" : "StageC";
        _loadingScreenAnimator.Play(anim);
    }

    public void FinishLoadingScreen(int sceneIndex)
    {
        var anim = sceneIndex == 1 ? "StageAOut" : sceneIndex == 2 ? "StageBOut" : "StageCOut";
        _loadingScreenAnimator.Play(anim);
    }
}
