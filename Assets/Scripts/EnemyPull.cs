using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EnemyPull : MonoBehaviour
{
    Stack<Enemy> _used = new Stack<Enemy>();
    Stack<Enemy> _free = new Stack<Enemy>();
    [SerializeField] private GameObject _enemyPrefab;

    public GameObject EnemyPrefab => _enemyPrefab;

    public Enemy GetEnemy()
    {
        if (_free.Count == 0)
        {
            Enemy enemy = Instantiate(_enemyPrefab).GetComponent<Enemy>();
            _used.Push(enemy);
            return enemy;
        }
        else
        {
            Enemy enemy = _free.Pop();
            _used.Push(enemy);
                
            enemy.gameObject.SetActive(true);
            enemy.Restart();
            return enemy;
        }
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        if (_used.Count != 0)
        {
            _used.Pop();
            _free.Push(enemy);
            enemy.gameObject.SetActive(false);
        }
    }
}
