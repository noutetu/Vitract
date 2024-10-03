using UnityEngine;

public class Magician : Character
{
    public Vector2 boxSize;  // 検知するボックスのサイズ
    public LayerMask detectionLayer; // 検知対象のレイヤー
    Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定

    protected override void Start()
    {
        // rangeを使ってboxSizeを初期化
        boxSize = new Vector2(range, 2);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        DetectObjects();
    }

    void DetectObjects()
    {
        // 現在の位置を中心にボックス範囲を設定
        detectionCenter = transform.position;
        // 指定したサイズとレイヤーで範囲内のすべてのオブジェクトを検出
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(detectionCenter, boxSize, 0f, detectionLayer);

        foreach (var hitCollider in hitColliders)
        {
            IDamageable detectedObject = hitCollider.GetComponent<IDamageable>();

            if (detectedObject != null && !enemies.Contains(detectedObject))
            {
                // 敵リストに追加
                enemies.Add(detectedObject);
                Debug.Log("検知したオブジェクト: " + hitCollider.name);

                // 検知した敵キャラクターを攻撃対象に設定
                enemyCharacter = detectedObject;

                // 攻撃対象が設定されたので、攻撃処理を開始
                AttackEvent();
            }
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
