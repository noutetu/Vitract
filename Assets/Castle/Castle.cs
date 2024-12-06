using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class Castle : MonoBehaviour, IDamageable
{
    private bool isDead; // キャラクターが死亡しているかどうか
    public bool IsDead => isDead; // 死亡状態の取得
    [SerializeField] float hp;
    [SerializeField] int cost;
    [SerializeField] bool isPlayer;
    public ReactiveProperty<float> currentHp{get;set;} = new ReactiveProperty<float>();           // 現在の体力
    [SerializeField] private HPBar hpBar;

    private void Start()
    {
        currentHp.Value = hp;
        hpBar.SetHP(currentHp.Value / hp);
    }
    public void TakeDamage(float damage)
    {
        if (isDead) { return; }
        currentHp.Value = Mathf.Max(currentHp.Value - damage, 0);
        hpBar.UpdateHP(currentHp.Value / hp);

        if (currentHp.Value <= 0)
        {
            GameManager.Instance.GameResult(isPlayer);
            isDead = true;
            Destroy(gameObject);
            return;
        }
    }
}
