﻿using System.Collections;
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

    public delegate void EnemyHit(Enemy enemy);
    public event EnemyHit OnEnemyHit;
    
    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied;
    
    #endregion
    
    public void Init()
    {
        if (_renderer == null || _spriteColor == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
            _spriteColor = _renderer.color;
        }
    }

    /// <summary>
    /// Восстанавливает начальные значения здоровья и очищает события
    /// </summary>
    public void Restart()
    {   
        Health = StartHealth;
        _renderer.color = _spriteColor;
        
        OnEnemyDied = delegate(Enemy enemy) {  };
        OnEnemyHit = delegate(Enemy enemy) {  };
    }
    
    private void OnDestroy()
    {
        OnEnemyDied = delegate(Enemy enemy) {  };
        OnEnemyHit = delegate(Enemy enemy) {  };
    }
    
    private void Update()
    {
        Vector2 direction = FollowedNPC.transform.position - transform.position;
        transform.up = direction;
        
        transform.Translate(0,  Speed*Time.deltaTime ,0);
    }

    private void OnMouseDown()
    {
        if (GameOptions.Instance.GameState != GameState.Paused && OnEnemyHit!=null)
        {
            OnEnemyHit(this);
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
