using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Renderer _renderer;
    public ProtectNPC FollowedNPC;
    public float Speed;
    public float Health;
    public float Damage;

    #region Events

    public delegate void EnemyDied(Enemy enemy);
    public event EnemyDied OnEnemyDied;
    
    #endregion
    
    private void Start()
    {
        OnEnemyDied += GameController.Instance.DestroyEnemy;
        _renderer = GetComponent<Renderer>();
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
            StartCoroutine(GetDamage(GameController.Instance.PlayerDamage));
        }
    }

    private IEnumerator GetDamage(float damage)
    {
        Health -= damage;
        _renderer.material.color = Color.black;
        yield return new WaitForSeconds(0.1f);
        _renderer.material.color = Color.white;
        
        if (Health <= 0)
        {
            if (OnEnemyDied != null)
            {
                OnEnemyDied(this);
            }
        }
    }
}
