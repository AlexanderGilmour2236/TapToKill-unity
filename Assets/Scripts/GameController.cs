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
    
    [SerializeField] private Image timeBar;
    /// <summary>
    /// Количество времени, необходимое для победы
    /// </summary>
    [SerializeField] private float startTime;
    /// <summary>
    /// Количество времени которое осталось до победы
    /// </summary>
    public float TimeLeft { get; private set; }
    public float StartTime => startTime;

    /// <summary>
    /// Урон, который игрок наносит кликом мыши
    /// </summary>
    public float PlayerDamage;
    public bool IsPaused { get; private set; }
    public bool IsGameRunning { get; private set; }
    
    public List<Enemy> Enemies { get; private set; }
    [SerializeField] private EnemyPull _enemyPull;
    public bool IsEnemySpawning;
    /// <summary>
    /// Время до следующего спавна противников
    /// </summary>
    private float _timeToSpawnLeft = 0;
    /// <summary>
    /// Количество времени через которое будут спавниться новые противники
    /// </summary>
    [SerializeField] private float spawnEvery = 2;
    /// <summary>
    /// Минимальное количество противников, которые могут спавниться за раз
    /// </summary>
    public int MinEnemiesToSpawn;
    /// <summary>
    /// Максимальное количество противников, которые могут спавниться за раз
    /// </summary>
    public int MaxEnemiesToSpawn;

    [SerializeField] private MainCamera _mainCamera;
    
    [SerializeField] private AudioSource _soundAudioSource;

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
        ProtectNPC.OnNpcDied += () => GameOver(GameOverType.Lost);

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
        background.transform.position = _mainCamera.transform.position;
        background.material.mainTextureOffset = background.transform.position;
        background.transform.Translate(0,0,10);

        // Таймер общего времени
        if (IsGameRunning && !IsPaused)
        {
            TimerTick();
        }
        // Спавн противников
        if (IsEnemySpawning)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// Вызывает GameOver(win) когда TimeLeft достигает нуля
    /// </summary>
    private void TimerTick()
    {
        TimeLeft -= Time.deltaTime;
        timeBar.fillAmount = TimeLeft / startTime;

        if (TimeLeft <= 0)
        {
            GameOver(GameOverType.Win);
        }
    }

    /// <summary>
    /// Спавнит несколько противников если _timeToSpawnLeft < 0
    /// </summary>
    private void SpawnEnemies()
    {
        _timeToSpawnLeft -= Time.deltaTime;
        
        // спавн противников
        if (_timeToSpawnLeft < 0)
        {
            // количество врагов которое будет спавниться
            int toSpawn = Random.Range(MinEnemiesToSpawn, MaxEnemiesToSpawn + 1);
            
            Vector2 npcPos = protectNpc.transform.position;
            
            Vector3 spawnPoint;
            // Расстояние от центра до края экрана по x
            float halfWidth = _mainCamera.CameraSize * Screen.width / Screen.height;
            
            Vector2 enemySize = _enemyPull.EnemyPrefab.GetComponent<SpriteRenderer>().size;
            
            for (int i = 0; i < toSpawn; i++)
            {
                // с какой стороны будет спавниться противник
                int side = Random.Range(1, 5);

                Enemy enemy = _enemyPull.GetEnemy();
                enemy.Init();
                
                if (side == 1)
                {
                    //top
                    enemy.transform.position = new Vector3(
                        Random.Range(-halfWidth + npcPos.x, npcPos.x + halfWidth),
                        npcPos.y + enemySize.y / 2 + _mainCamera.CameraSize + Random.Range(0, 3),
                        0);
                }
                else if (side == 2)
                {
                    //right
                    enemy.transform.position = new Vector3(
                        npcPos.x + halfWidth + enemySize.x / 2 + Random.Range(0, 3),
                        Random.Range(-_mainCamera.CameraSize - enemySize.x / 2 + npcPos.y,
                            _mainCamera.CameraSize + npcPos.y + enemySize.x / 2),
                        0);
                }
                else if (side == 3)
                {
                    //bottom
                    enemy.transform.position = new Vector3(
                        Random.Range(-halfWidth + npcPos.x, npcPos.x + halfWidth),
                        npcPos.y - enemySize.y / 2 - _mainCamera.CameraSize - Random.Range(0, 3),
                        0);
                }
                else if(side == 4)
                {
                    //left
                    enemy.transform.position = new Vector3(
                        npcPos.x - halfWidth - enemySize.x / 2 - Random.Range(0, 3),
                        Random.Range(-_mainCamera.CameraSize - enemySize.x / 2 + npcPos.y,
                            _mainCamera.CameraSize + npcPos.y + enemySize.x / 2),
                        0);
                }

                enemy.AudioSource = _soundAudioSource;
                enemy.FollowedNPC = protectNpc;
                
                enemy.Restart();
                
                enemy.OnEnemyDied += DestroyEnemy;
                enemy.OnEnemyHit += HitEnemy;
                
                Enemies.Add(enemy);
            }
            
            _timeToSpawnLeft = spawnEvery;
        }
    }

    public void GameStart()
    {
        if (OnGameStart != null)
        {
            OnGameStart();
        }
        ClearEnemies();
        GameOptions.Instance.GameState = GameState.Playing;
        IsEnemySpawning = true;
        _timeToSpawnLeft = 0;
        TimeLeft = startTime;
        IsGameRunning = true;
    }

    public void Pause(bool pauseParameter)
    {   
        if (pauseParameter)
        {
            Time.timeScale = 0;
            GameOptions.Instance.GameState = GameState.Paused;
        }
        else
        {
            GameOptions.Instance.GameState = GameState.Playing;
            Time.timeScale = 1;
        }

        IsPaused = pauseParameter;
        
        if (OnPause != null)
        {
            OnPause(pauseParameter);
        }
    }
    
    private void GameOver(GameOverType gameOverType)
    {
        IsEnemySpawning = false;
        IsGameRunning = false;
        
        ClearEnemies();
        if (OnGameOver != null)
        {
            OnGameOver(gameOverType);
        }
    }
    
    public void ToMainMenu()
    {
        GameOptions.Instance.GameState = GameState.MainMenu;
        
        IsEnemySpawning = false;
        IsGameRunning = false;
        
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
    /// Очищает список противников и возвращает их в пул
    /// </summary>
    private void ClearEnemies()
    {
        foreach (Enemy enemy in Enemies)
        {
            _enemyPull.ReleaseEnemy(enemy);
        }

        Enemies.Clear();
    }

    /// <summary>
    /// Атакует противника
    /// </summary>
    /// <param name="enemy"></param>
    public void HitEnemy(Enemy enemy)
    {
        enemy.TakeDamage(PlayerDamage);
    }
    
    /// <summary>
    /// Возвразает противника в пул и убирает его из контейнера
    /// </summary>
    /// <param name="enemy">Противник</param>
    public void DestroyEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
        _enemyPull.ReleaseEnemy(enemy);
    }
    
}


