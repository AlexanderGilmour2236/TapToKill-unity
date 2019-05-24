using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    #region Singleton

    private static GameController _instance;
    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject ins = new GameObject("GameController");
                ins.AddComponent<GameController>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    #endregion

    public enum GameOverType
    {
        Win, Lost
    }
    
    [SerializeField] private ProtectNPC protectNpc;
    [SerializeField] private Renderer background;
    [SerializeField] private GameObject enemyPrefab;
    /// <summary>
    /// Урон, который игрок наносит кликом мыши
    /// </summary>
    public float PlayerDamage;
    public List<Enemy> Enemies { get; private set; }
    public bool IsPaused { get; private set; }
    
    public bool IsEnemySpawning;
    /// <summary>
    /// Время до следующего спавно противников
    /// </summary>
    private float _timeToSpawnLeft = 0;
    /// <summary>
    /// Количество времени через которое будут спавниться новые противники
    /// </summary>
    private float _spawnEvery = 2;
    /// <summary>
    /// Минимальное количество противников, которые могут спавниться за раз
    /// </summary>
    public int MinEnemiesToSpawn;
    /// <summary>
    /// Максимальное количество противников, которые могут спавниться за раз
    /// </summary>
    public int MaxEnemiesToSpawn;
    
    #region Events

    public delegate void onMainMenu();
    public static event onMainMenu OnMainMenu;
    
    public delegate void onGameOver(GameOverType gameOverType);
    public static event onGameOver OnGameOver;
    
    public delegate void onGameStart();
    public static event onGameStart OnGameStart;

    public delegate void onPause(bool setPause);
    public static event onPause OnPause;
    
    
    
    #endregion
    
    private void Start()
    {      
        ProtectNPC.OnNpcHit += DestroyEnemy;
        ProtectNPC.OnNpcDied += GameOver;
        
        Enemies = new List<Enemy>();
        IsEnemySpawning = false;
        
        ToMainMenu();
    }
    
    private void OnDestroy()
    {
        OnGameOver = delegate(GameOverType type) {  };
        OnGameStart = delegate {  };
        OnPause = delegate(bool pause) {  };
        OnMainMenu = delegate {  };
    }
    
    private void Update()
    {
        background.transform.position = MainCamera.Instance.transform.position;
        background.material.mainTextureOffset = MainCamera.Instance.transform.position;
        background.transform.Translate(0,0,10);
        if (Input.GetKey(KeyCode.R))
        {
            RestartScene();
        }
        
        // Спавн противников
        if (IsEnemySpawning)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Спавнит несколько противников если _timeToSpawnLeft < 0
    /// </summary>
    private void SpawnEnemies()
    {
        _timeToSpawnLeft -= Time.deltaTime;
        
        if (_timeToSpawnLeft < 0)
        {
            int toSpawn = Random.Range(MinEnemiesToSpawn, MaxEnemiesToSpawn);
            
            Vector2 npcPos = protectNpc.transform.position;
            Vector3 spawnPoint;
            for (int i = 0; i < toSpawn; i++)
            {
                
                Enemy enemy = Instantiate(
                    enemyPrefab, new Vector3(
                        Random.Range(-20 + npcPos.x, npcPos.x + 20),
                        Random.Range(-20 + npcPos.y, 20 + npcPos.y),
                        0), new Quaternion(0, 0, 0, 0)
                ).GetComponent<Enemy>();

                enemy.FollowedNPC = protectNpc;

                Enemies.Add(enemy);
            }

            _timeToSpawnLeft = _spawnEvery;
        }
    }

    public void GameStart()
    {
        if (OnGameStart != null)
        {
            OnGameStart();
        }
        ClearEnemies();
        IsEnemySpawning = true;
        _timeToSpawnLeft = 0;
    }

    public void Pause(bool pauseParameter)
    {
        if (pauseParameter)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        IsPaused = pauseParameter;
        
        if (OnPause != null)
        {
            OnPause(pauseParameter);
        }
    }
    
    private void GameOver()
    {
        IsEnemySpawning = false;
        ClearEnemies();
        if (OnGameOver != null)
        {
            OnGameOver(GameOverType.Lost);
        }
    }
    
    public void ToMainMenu()
    {
        IsEnemySpawning = false;
        ClearEnemies();
        protectNpc.RestartNPC();
        if (IsPaused)
        {
            Pause(false);
        }
        if (OnMainMenu != null)
        {
            OnMainMenu();
        }

    }
    /// <summary>
    /// Очищает список противников и удаляет их из сцены
    /// </summary>
    private void ClearEnemies()
    {
        foreach (Enemy enemy in Enemies)
        {
            Destroy(enemy.gameObject);
        }

        Enemies.Clear();
    }

    void RestartScene()
    {
        SceneManager.LoadScene("Game");
    }
    
    /// <summary>
    /// Уничтожает противника и убирает его из контейнера
    /// </summary>
    /// <param name="enemy">Противник</param>
    public void DestroyEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

}
