using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    
    void Start()
    {
        GameController.OnPause += HideButton;
        GameController.OnGameStart += OnGameStart;
        GameController.OnGameOver += OnGameOver;
        GameController.OnMainMenu += OnMainMenu;
        HideButton(true);
    }

    private void HideButton(bool hideParameter)
    {
        gameObject.SetActive(!hideParameter);
    }

    
    private void OnMainMenu()
    {
        HideButton(true);
    }

    private void OnGameStart()
    {
        HideButton(false);
    }

    private void OnGameOver(GameController.GameOverType gameOverType)
    {
        HideButton(true);
    }
   
}
