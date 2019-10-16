using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    enum MenuType
    {
        MainMenu, PauseMenu, GameOverMenu, WinMenu
    }

    [SerializeField] private Text mainLabel;
    [SerializeField] private VerticalLayoutGroup buttonsLayoutGroup;
    private Button[] _buttons;
    void Start()
    {
        GameController.OnGameStart += OnGameStart;
        GameController.OnGameOver += OnGameOver;
        GameController.OnPause += OnPause;
        GameController.OnMainMenu += OnMainMenu;
        _buttons = buttonsLayoutGroup.GetComponentsInChildren<Button>();
    }

    private void OnMainMenu()
    {
        ShowMenu(MenuType.MainMenu);
    }
    
    private void OnGameOver(GameController.GameOverType gameOverType)
    {
        if (gameOverType == GameController.GameOverType.Lost)
        {
            ShowMenu(MenuType.GameOverMenu);
        }
        else
        {
            ShowMenu(MenuType.WinMenu);
        }
    }
    
    private void OnPause(bool showParameter)
    {
        if (showParameter)
        {
            ShowMenu(MenuType.PauseMenu);
        }
        else
        {
            OnGameStart();
        }
    }    
    
    private void ShowMenu(MenuType menuType)
    {
        if (menuType == MenuType.MainMenu)
        {
            mainLabel.text = "TapToKill";
            mainLabel.fontSize = 87;
            _buttons[1].gameObject.SetActive(false);
            _buttons[0].GetComponentInChildren<Text>().text = "START";
            
            _buttons[0].onClick.RemoveAllListeners();
            _buttons[0].onClick.AddListener(GameController.Instance.GameStart);
        } 
        else 
        if (menuType == MenuType.PauseMenu)
        {
            mainLabel.text = "PAUSE";
            mainLabel.fontSize = 87;
            _buttons[1].gameObject.SetActive(true);
            _buttons[0].GetComponentInChildren<Text>().text = "RESUME";
            
            _buttons[0].onClick.RemoveAllListeners();
            _buttons[0].onClick.AddListener(() => GameController.Instance.Pause(false));
            
            _buttons[1].GetComponentInChildren<Text>().text = "MAIN MENU";
            
        }
        else if (menuType == MenuType.GameOverMenu)
        {
            TimeSpan span = TimeSpan.FromSeconds(GameController.Instance.StartTime - GameController.Instance.TimeLeft);
            DateTime time = new DateTime(2019, 1, 1, 0, 0, 0);
            time += span;

            mainLabel.text = string.Format("YOU'VE SURVIVED {0}s", time.ToString("m:ss"));
            
            mainLabel.fontSize = 68;
            _buttons[1].gameObject.SetActive(true);
            _buttons[0].GetComponentInChildren<Text>().text = "TRY AGAIN";
            
            _buttons[0].onClick.RemoveAllListeners();
            _buttons[0].onClick.AddListener(GameController.Instance.GameStart);
            
            _buttons[1].GetComponentInChildren<Text>().text = "MAIN MENU";
        }
        else
        if(menuType == MenuType.WinMenu)
        {
            mainLabel.text = "YOU WIN";
            mainLabel.fontSize = 87;
            _buttons[1].gameObject.SetActive(true);
            _buttons[0].GetComponentInChildren<Text>().text = "RESTART";
            
            _buttons[0].onClick.RemoveAllListeners();
            _buttons[0].onClick.AddListener(GameController.Instance.GameStart);
            
            _buttons[1].GetComponentInChildren<Text>().text = "MAIN MENU";
        }
        
        gameObject.SetActive(true);
    }
    
    void OnGameStart()
    {
        gameObject.SetActive(false);
    }
}
