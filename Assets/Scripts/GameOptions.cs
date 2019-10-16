using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOptions
{
    #region Singleton

    private static GameOptions _instance;

    public static GameOptions Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameOptions();
            }

            return _instance;
        }
    }

    #endregion

    public GameState GameState { get; set; }

}
