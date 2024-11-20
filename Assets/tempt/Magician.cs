using UniRx;
using UnityEngine;

public class Magician : Character
{
    public Vector2 boxSize;  // 検知するボックスのサイズ
    public LayerMask enemyLayer; // 検知対象のレイヤー
    public LayerMask playerLayer; // 検知対象のレイヤー
    public LayerMask targetLayer; // 検知対象のレイヤー
    Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定

    // TODO なんか二体目以降にうまく遠距離攻撃できない（接触するといける）
    protected override void Start()
    {
        // rangeを使ってboxSizeを初期化
        boxSize = new Vector2(range, 2);
        //攻撃対象にするレイヤーを設定
        targetLayer = enemyLayer;
        if (!isPlayer) { targetLayer = playerLayer; }
    }

    protected override void FixedUpdate()
    {
        DetectObjects();
        base.FixedUpdate();
    }

    void DetectObjects()
    {
        // 現在の位置を中心にボックス範囲を設定
        detectionCenter = transform.position;
        // 指定したサイズとレイヤーで範囲内のすべてのオブジェクトを検出
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(detectionCenter, boxSize, 0f, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            IDamageable detectedObject = hitCollider.GetComponent<IDamageable>();

            // 敵リストに追加

            targetList.RegisterAtEnemies(detectedObject);
            // HPが0以下になったときにリストから削除する購読を追加
            detectedObject.currentHp
                //.Skip(1) // 初期値をスキップして、変化があった時のみ反応
                .Where(hp => hp <= 0)
                .Subscribe(_ =>
                {
                    enemyObject = null;
                    targetList.RemoveInEnemies(detectedObject);
                })
                .AddTo(this); // 購読を管理リストに追加

            // 最初の敵キャラクターを攻撃対象とする
            enemyObject = targetList.SetNextEnemy();
            //敵キャラクターがいて、現在攻撃中でなければ
            if (enemyObject != null && canAttack)
            {
                AttackEvent();  // 攻撃イベントの開始
            }


            Debug.Log("検知したオブジェクト: " + hitCollider.name);
        }

        // デバッグ用に範囲を可視化
        Debug.DrawRay(detectionCenter, Vector2.right * boxSize.x / 2, Color.red);
        Debug.DrawRay(detectionCenter, Vector2.up * boxSize.y / 2, Color.red);
    }

    // シーン上で可視化するためのメソッド
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}

