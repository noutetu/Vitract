using UniRx;
using UnityEngine;
using System;

public class Magician : Character
{
    // ================= フィールド =================
    Vector2 boxSize;  // 検知するボックスのサイズ
    [SerializeField] private LayerMask enemyLayer; // 敵キャラクターのレイヤー
    [SerializeField] private LayerMask playerLayer; // プレイヤーキャラクターのレイヤー
    private LayerMask targetLayer; // 検知対象のレイヤー（敵か味方か）
    private Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定
    private AttackRange attackRange; // 攻撃範囲の処理を行うクラス

    // ================= Unity ライフサイクル =================
    protected override void Start()
    {
        Debug.Log("Magicianです");
        // ボックスサイズを設定（横幅はrangeを使い、縦幅は固定値）
        boxSize = new Vector2(range, 2);
        
        // 攻撃対象にするレイヤーを設定（プレイヤーであれば敵レイヤー、敵であればプレイヤーレイヤー）
        targetLayer = isPlayer ? enemyLayer : playerLayer;
        
        // AttackRangeクラスのインスタンスを初期化し、敵を検知した際にリストに追加する
        attackRange = new AttackRange(AddEnemyToList);

        // 一定間隔で敵を検知する処理を設定
        Observable.Interval(TimeSpan.FromMilliseconds(50)) // 50msごとに検知を行う
            .Subscribe(_ => attackRange.DetectEnemy(detectionCenter, boxSize, targetLayer))
            .AddTo(this); // 購読の解除をOnDestroyで行うように設定
    }

    protected override void FixedUpdate()
    {
        // 現在のキャラクター位置を検知の中心に設定
        detectionCenter = transform.position;
        base.FixedUpdate(); // 親クラスのFixedUpdateメソッドを呼び出す
    }

}

// ================= クラス説明 =================
// このクラスはMagicianキャラクターの動作を管理します。
// 特に、敵の検知範囲の設定と、一定間隔での敵検知処理を行います。
// Gizmosを使用して、シーンビューで検知範囲を可視化できるようにしています。
