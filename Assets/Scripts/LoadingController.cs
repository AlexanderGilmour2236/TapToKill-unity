using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Connect(LoadGame));
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync("Game");
    }
    
    private IEnumerator Connect(Action onComplete)
    {
      WWWForm form = new WWWForm();
      form.AddField("test", "connect");
      WWW www = new WWW("https://taptokill.000webhostapp.com/",form);
      
      yield return www;
      if (www.error != null)
      {
          StartCoroutine(Connect());
          yield break;
      }
      Debug.Log(www.text);
      
      if (www.text == "connected")
      {
          onComplete();
      }
    }
}
