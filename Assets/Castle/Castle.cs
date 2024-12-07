using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;


[RequireComponent(typeof(AudioSource))]
public class Castle : MonoBehaviour, IDamageable
{
    private bool isAnimating;
    private bool isDead; // キャラクターが死亡しているかどうか
    public bool IsDead => isDead; // 死亡状態の取得

    [SerializeField] float hp;
    [SerializeField] int cost;
    [SerializeField] bool isPlayer;
    public ReactiveProperty<float> currentHp{get;set;} = new ReactiveProperty<float>();           // 現在の体力
    
    SpriteRenderer[] spriteRenderers;
    [SerializeField] private HPBar hpBar;
    private AudioSource audioSource;
    [SerializeField] AudioClip damegeSound;

    private void Start()
    {
        currentHp.Value = hp;
        hpBar.SetHP(currentHp.Value / hp);
        audioSource = GetComponent<AudioSource>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }
    public void TakeDamage(float damage)
    {
        if (isDead) { return; }
        currentHp.Value = Mathf.Max(currentHp.Value - damage, 0);
        hpBar.UpdateHP(currentHp.Value / hp);
        DamageAnimation();

        if (currentHp.Value <= 0)
        {
            GameManager.Instance.GameResult(isPlayer);
            isDead = true;
            Destroy(gameObject);
            DOTween.Kill(this);
            return;
        }
    }
    public void DamageAnimation()
    {
        if (isAnimating) return; // アニメーション中なら処理をスキップ

        isAnimating = true; // フラグを設定
        int animationsRemaining = spriteRenderers.Length; // アニメーションが終了するスプライトの数をカウント

        foreach (var spriteRenderer in spriteRenderers)
        {
            // 元の色を保持
            Color originalColor = spriteRenderer.color;

            // 点滅アニメーション (黒に変化させて戻る)
            spriteRenderer.DOColor(Color.black, 0.1f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    spriteRenderer.color = originalColor;

                    // 全てのアニメーションが終了したらフラグをリセット
                    animationsRemaining--;
                    if (animationsRemaining == 0)
                    {
                        isAnimating = false;
                    }
                });
        }
    }
}
