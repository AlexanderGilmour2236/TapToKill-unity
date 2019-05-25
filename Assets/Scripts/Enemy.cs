using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer _renderer;
    /// <summary>
    /// Объект за которым следует противник
    /// </summary>
    public ProtectNPC FollowedNPC;
    public float Speed;
    public float Health;
    public float StartHealth;
    public float Damage;
    /// <summary>
    /// Стартовый цвет спрайта
    /// </summary>
    private Color _spriteColor;
    
    public AudioSource AudioSource;
    [SerializeField] private AudioClip hitSound;
    
    
    #region Events

    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied;
    
    #endregion
    
    private void Start()
    {
        OnEnemyDied += GameController.Instance.DestroyEnemy;
        _renderer = GetComponent<SpriteRenderer>();
        _spriteColor = _renderer.color;
    }

    public void Restart()
    {   
        Health = StartHealth;
        _renderer.color = _spriteColor;
    }
    
    private void OnDestroy()
    {
        OnEnemyDied = delegate(Enemy enemy) {  };
    }
    
    private void Update()
    {
        Vector2 direction = FollowedNPC.transform.position - transform.position;
        transform.up = direction;
        
        transform.Translate(0,  Speed*Time.deltaTime ,0);
    }

    private void OnMouseDown()
    {
        if (!GameController.Instance.IsPaused)
        {
            TakeDamage(GameController.Instance.PlayerDamage);
        }
    }

    /// <summary>
    /// Получает урон, при достижении здоровья 0 вызывает OnEnemyDied
    /// </summary>
    /// <param name="damage">Получаемый урон</param>
    public void TakeDamage(float damage)
    {
        StartCoroutine(TakeDamageCoroutine(damage));
    }
    
    
    private IEnumerator TakeDamageCoroutine(float damage)
    {
        Health -= damage;
        _renderer.color = Color.white;
        AudioSource.PlayOneShot(hitSound);
        
        yield return new WaitForSeconds(0.1f);
        _renderer.color = _spriteColor;
        if (Health <= 0)
        {
            if (OnEnemyDied != null)
            {
                OnEnemyDied(this);
            }
        }
    }
}
