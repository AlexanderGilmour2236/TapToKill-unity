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
            // Расстояние от центра до края экрана по x
            float halfWidth = MainCamera.Instance.CameraSize * Screen.width / Screen.height;
            Vector2 enemySize = enemyPrefab.GetComponent<SpriteRenderer>().size;
            
            for (int i = 0; i < toSpawn; i++)
            {
                // с какой стороны будет спавниться противник
                int side = Random.Range(1, 4);

                Enemy enemy;
                if (side == 1)
                {
                    //top
                    enemy = Instantiate(
                        enemyPrefab, new Vector3(
                        Random.Range(-halfWidth + npcPos.x, npcPos.x + halfWidth),
                        npcPos.y + enemySize.y/2 + MainCamera.Instance.CameraSize + Random.Range(0,3),
                        0), new Quaternion(0, 0, 0, 0)
                    ).GetComponent<Enemy>();
                }
                else if (side == 2)
                {
                    //right
                    enemy = Instantiate(
                        enemyPrefab, new Vector3(
                            npcPos.x + halfWidth + enemySize.x / 2 + Random.Range(0,3),
                            Random.Range(-MainCamera.Instance.CameraSize - enemySize.x/2 + npcPos.y, MainCamera.Instance.CameraSize + npcPos.y + enemySize.x/2),
                            0), new Quaternion(0, 0, 0, 0)
                    ).GetComponent<Enemy>();
                }
                else if (side == 3)
                {
                    //bottom
                    enemy = Instantiate(
                        enemyPrefab, new Vector3(
                            Random.Range(-halfWidth + npcPos.x, npcPos.x + halfWidth),
                            npcPos.y - enemySize.y/2 - MainCamera.Instance.CameraSize - Random.Range(0,3),
                            0), new Quaternion(0, 0, 0, 0)
                    ).GetComponent<Enemy>();
                }
                else if(side == 4)
                {
                    //left
                    enemy = Instantiate(
                        enemyPrefab, new Vector3(
                            npcPos.x - halfWidth - enemySize.x / 2 - Random.Range(0,3),
                            Random.Range(-MainCamera.Instance.CameraSize - enemySize.x/2 + npcPos.y, MainCamera.Instance.CameraSize + npcPos.y + enemySize.x/2),
                            0), new Quaternion(0, 0, 0, 0)
                    ).GetComponent<Enemy>();
                }
                else
                {
                    enemy = new Enemy();
                }

                enemy.AudioSource = _soundAudioSource;
                enemy.FollowedNPC = protectNpc;

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
