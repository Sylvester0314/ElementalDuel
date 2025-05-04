using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.UI.Misc.Transition;
using Shared.Handler;
using Shared.Misc;
using UnityEngine.EventSystems;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Components")] 
    public AudioListener audioListener;
    public EventSystem eventSystem;
    public List<CanvasGroup> uis;

    [Header("Inner Configurations")] 
    public string key;
    public float fadeDuration = 1f;

    [Header("Callbacks")] 
    public List<MonoBehaviour> onSceneUnloadHandlers;

    private bool _lazyFinishLoadFlag;
    private string _currentLoadedScene;
    private AbstractTransition _transition;
    
    public void LoadScene(
        string sceneName,
        LoadSceneMode mode = LoadSceneMode.Additive,
        bool lazyLoad = false,
        bool allowSceneActivation = true,
        AbstractTransition transition = null,
        Func<AbstractTransition, Task> beforeFadeIn = null,
        Func<AbstractTransition, Task> onSceneLoaded = null
    )
    {
        _transition = transition ?? FixedScene.Instance.dark;
        
        StartCoroutine(LoadSceneWithFade(
            sceneName, mode, lazyLoad, 
            allowSceneActivation, beforeFadeIn, onSceneLoaded
        ));
        SetListenerStatus(false);
        uis.ForEach(ui => ui.blocksRaycasts = false);
    }

    public void UnloadCurrentScene(Action afterSceneUnload = null)
    {
        if (!string.IsNullOrEmpty(_currentLoadedScene))
            StartCoroutine(UnloadSceneWithFade(_currentLoadedScene, afterSceneUnload));
    }

    public void ActiveFlag()
    {
        _lazyFinishLoadFlag = true;
    }

    private IEnumerator LoadSceneWithFade(
        string sceneName, LoadSceneMode mode, 
        bool lazyLoad, bool allowSceneActivation, 
        Func<AbstractTransition, Task> beforeFadeIn,
        Func<AbstractTransition, Task> onSceneLoaded
    )
    {
        if (mode == LoadSceneMode.Single)
            DontDestroyOnLoad(gameObject);
        
        if (beforeFadeIn != null)
            yield return StaticMisc.AwaitAsync(beforeFadeIn.Invoke(_transition));
        
        yield return StartCoroutine(_transition.FadeIn(fadeDuration));
        
        var operation = SceneManager.LoadSceneAsync(sceneName, mode);
        if (operation != null)
            operation.allowSceneActivation = allowSceneActivation;
        
        while (operation is { isDone: false })
            yield return null;
        
        _currentLoadedScene = sceneName;

        if (onSceneLoaded != null)
            yield return StaticMisc.AwaitAsync(onSceneLoaded.Invoke(_transition));
        
        if (lazyLoad)
        {
            while (!_lazyFinishLoadFlag)
                yield return null;
        }

        yield return StartCoroutine(_transition.FadeOut(fadeDuration));

        _lazyFinishLoadFlag = false;
        if (mode == LoadSceneMode.Single)
            Destroy(gameObject);
    }

    private IEnumerator UnloadSceneWithFade(
        string sceneName, Action afterSceneUnload = null)
    {
        yield return StartCoroutine(_transition.FadeIn(fadeDuration));

        var operation = SceneManager.UnloadSceneAsync(sceneName);

        while (operation is { isDone: false })
            yield return null;

        _currentLoadedScene = null;

        onSceneUnloadHandlers.ForEach(handler =>
        {
            if (handler is ISceneUnloadHandler onUnload)
                onUnload.OnSceneUnload();
        });
        
        yield return StartCoroutine(_transition.FadeOut(fadeDuration));

        afterSceneUnload?.Invoke();
        
        SetListenerStatus(true);
        uis.ForEach(ui => ui.blocksRaycasts = true);
    }

    private void SetListenerStatus(bool status)
    {
        if (audioListener != null)
            audioListener.enabled = status;

        if (eventSystem != null)
            eventSystem.gameObject.SetActive(status);
    }
}