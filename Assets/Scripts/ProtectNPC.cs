using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ProtectNPC : MonoBehaviour
{
    #region Singleton
    
    private ProtectNPC(){}

    private static ProtectNPC _instance;

    public static ProtectNPC Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject ins = new GameObject("ProtectNPC");
                ins.AddComponent<ProtectNPC>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    #endregion
    
    public float Speed;
    public float RotationSpeed;
    public float Health { get; private set; }
    public float StartHealth;
    private Vector2 _targetDirection;
    
    private List<Vector3> _pathPoits;
    private PointPull _pointPull;
    [SerializeField] private LineRenderer lineRenderer;
    
    [SerializeField] private UnityEngine.UI.Image healthBar;
    private Coroutine showHealthCoroutine;
    [SerializeField] private float healthBarYOffset;
    
    /// <summary>
    /// Минимальное отклонение новой точки пути по оси x от предыдущей, по модулю
    /// </summary>
    public float MinPointOffsetX;
    
    /// <summary>
    /// Максимальное отклонение новой точки пути от предыдущей по модулю
    /// </summary>
    public Vector2 MaxPointOffset;
    
    #region events

        public delegate void GetHit(Enemy enemy);
        /// <summary>
        /// Происходит при столкновении противника
        /// </summary>
        public static event GetHit OnNpcHit;
    
        public delegate void NpcDied();
        /// <summary>
        /// Происходит, когда health < 0
        /// </summary>
        public static event NpcDied OnNpcDied;

    #endregion
    
    private void Start()
    {
        int seed = Random.Range(0, 255);
        Random.InitState(seed);
        Debug.Log("Seed: "+ seed);
        OnNpcHit += TakeDamage;
        GameController.OnGameStart += RestartNPC;
        
        _pointPull = new PointPull();
        _pathPoits = new List<Vector3>();

        RestartNPC();
    }

    public void RestartNPC()
    {
        Health = StartHealth;
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        OnNpcHit -= TakeDamage;
        GameController.OnGameStart -= RestartNPC;
        
        OnNpcHit = delegate(Enemy enemy) {  };
        OnNpcDied = delegate {  };
    }

    private void Update()
    {
        transform.localEulerAngles += Vector3.back*Time.deltaTime * RotationSpeed;
        GeneratePath();
        Vector3 NextPoint = transform.position;
        foreach (Vector3 point in _pathPoits)
        {
            if (point.x > NextPoint.x)
            {
                NextPoint = point;
                break;
            }
        }
        
        Vector2 direction = NextPoint - transform.position;
        direction = direction.normalized * Speed * Time.deltaTime;
        
        transform.Translate(direction,Space.World);
        healthBar.transform.position = transform.position;
        healthBar.transform.Translate(0,healthBarYOffset,0);
    }
    /// <summary>
    /// Добавляет в массив _pathPoint новую точку если последняя точка в списке не дальше чем MainCamera.maxCameraSize по оси x,
    /// и удаляет первую, если она дальше чем -MainCamera.maxCameraSize по оси x
    /// </summary>
    void GeneratePath()
    {
        // Удаление первой точки
        if (_pathPoits.Count > 1)
        {

            if (_pathPoits[1].x < MainCamera.Instance.transform.position.x - MainCamera.Instance.maxCameraSize * Screen.width/Screen.height)
            {
                _pointPull.ReleasePoint(_pathPoits[0]);
                _pathPoits.Remove(_pathPoits[0]);
                
                lineRenderer.SetPositions(_pathPoits.ToArray());
            }  
        }
        
        // Добавление новой точки
        Vector3 point;
        int count = _pathPoits.Count;
        
        if (count == 0)
        {
            point = _pointPull.GetPoint();
            
            point.x = 0;
            point.y = 0;
            
            _pathPoits.Add(point);
        }
        else
        {
            if (_pathPoits[_pathPoits.Count - 1].x < transform.position.x + MainCamera.Instance.maxCameraSize  * Screen.width/Screen.height)
            {
                point = _pointPull.GetPoint();
            
                float lastX = _pathPoits[_pathPoits.Count - 1].x;
                float lastY = _pathPoits[_pathPoits.Count - 1].y;
        
                point.x = Random.Range(lastX + MinPointOffsetX, lastX + MaxPointOffset.x);
                point.y = Random.Range(lastY - MaxPointOffset.y, lastY + MaxPointOffset.y);
            
                _pathPoits.Add(point);
            }

        }

        if (count != _pathPoits.Count)
        {
            lineRenderer.positionCount = _pathPoits.Count;
            lineRenderer.SetPositions(_pathPoits.ToArray());  
        }
    }
    
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.tag == "enemy")
        {
            if (OnNpcHit!=null)
            {
                OnNpcHit(other.collider.GetComponent<Enemy>());   
            }
        }
    }

    private void TakeDamage(Enemy enemy)
    {
        Health -= enemy.Damage;
        enemy.TakeDamage(enemy.Health);
        
        if (showHealthCoroutine != null)
        {
            StopCoroutine(showHealthCoroutine);
        }
        showHealthCoroutine = StartCoroutine(ShowHealth());
        if (Health <= 0)
        {
            if (OnNpcDied != null)
            {
                OnNpcDied();
            }

            Die();
        }
    }

    /// <summary>
    /// Показывает колличество HP и скрывает их через 1 секунду
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowHealth()
    {
        healthBar.enabled = true;
        healthBar.fillAmount = Health / StartHealth;
        yield return new WaitForSeconds(1);
        healthBar.enabled = false;
    }
    
    private void Die()
    {
        gameObject.SetActive(false);
    }
}


public class PointPull
{
    Stack<Vector3> _used = new Stack<Vector3>();
    Stack<Vector3> _free = new Stack<Vector3>();

    public Vector3 GetPoint()
    {
        if (_free.Count == 0)
        {
            Vector3 point = new Vector3();
            _used.Push(point);
            return point;
        }
        else
        {
            Vector3 point = _free.Pop();
            _used.Push(point);
            return point;
        }
    }

    public void ReleasePoint(Vector3 point)
    {
        _used.Pop();
        _free.Push(point);
    }
}

