using UniRx;
using UnityEngine;
using System;

public class Magician : Character
{
    [SerializeField] Vector2 boxSize;  // 検知するボックスのサイズ
    [SerializeField] private LayerMask enemyLayer; // 検知対象のレイヤー
    [SerializeField] private LayerMask playerLayer; // 検知対象のレイヤー
    private LayerMask targetLayer; // 検知対象のレイヤー
    private Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定
    private AttackRange attackRange;


    protected override void Start()
    {
        // rangeを使ってboxSizeを初期化
        boxSize = new Vector2(range, 2);
        //攻撃対象にするレイヤーを設定
        targetLayer = enemyLayer;
        if (!isPlayer) { targetLayer = playerLayer; }
        attackRange = new AttackRange(AddEnemyToList);

        Observable.Interval(TimeSpan.FromMilliseconds(50))  // 500msごとに実行
    .Subscribe(_ => attackRange.DetectEnemy(detectionCenter, boxSize, targetLayer))
    .AddTo(this);

    }

    protected override void FixedUpdate()
    {
        detectionCenter = transform.position;
        base.FixedUpdate();
    }
    // シーン上で可視化するためのメソッド
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}

