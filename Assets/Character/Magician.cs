using Unity.VisualScripting;
using UnityEngine;

public class Magician : Character
{
    public Vector2 boxSize;  // 検知するボックスのサイズ
    public LayerMask detectionLayer; // 検知対象のレイヤー
    Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定
    Collider2D[] hitColliders; // 指定したサイズとレイヤーで範囲内のすべてのオブジェクトを検出
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
        hitColliders = Physics2D.OverlapBoxAll(detectionCenter, boxSize, 0f, detectionLayer);

        // 検出されたオブジェクトを全てループで処理
        foreach (var hitCollider in hitColliders)
        {
            IDamageable detectedObject = hitCollider.GetComponent<IDamageable>();
            
            detectedObject.TakeDamageAndCheckDead(1);            
            /*
            ラピスみたいなDotダメージ！！！
            */
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
