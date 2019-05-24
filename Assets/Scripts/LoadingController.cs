using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [SerializeField] private Image loadingBar;

    private AsyncOperation _loading;
    private void Start()
    {
        StartCoroutine(Connect(LoadGame));
    }

    private void Update()
    {
        if (_loading != null)
        {
            loadingBar.fillAmount = _loading.progress;
        }
    }

    public void LoadGame()
    {
        _loading = SceneManager.LoadSceneAsync("Game");
    }
    
    private IEnumerator Connect(Action onComplete)
    {
      WWWForm form = new WWWForm();
      form.AddField("test", "connect");
      WWW www = new WWW("https://taptokill.000webhostapp.com/",form);
      
      yield return www;
      if (www.error != null)
      {
          Debug.Log(www.error);
          yield break;
      }
      Debug.Log(www.text);
      
      if (www.text == "connected")
      {
          onComplete();
      }
    }
}
