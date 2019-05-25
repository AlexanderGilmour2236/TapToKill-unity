using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer _renderer;
    public ProtectNPC FollowedNPC;
    public float Speed;
    public float Health;
    public float Damage;
    public Color SpriteColor;
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
        _renderer.color = SpriteColor;
        if (Health <= 0)
        {
            if (OnEnemyDied != null)
            {
                OnEnemyDied(this);
            }
        }
    }
}
