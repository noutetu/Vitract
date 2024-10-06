using UnityEngine;

public class Magician : Character
{
    public Vector2 boxSize;  // 検知するボックスのサイズ
    public LayerMask enemyLayer; // 検知対象のレイヤー
    public LayerMask playerLayer; // 検知対象のレイヤー
    public LayerMask targetLayer; // 検知対象のレイヤー
    Vector2 detectionCenter;  // 現在の位置を中心にボックス範囲を設定

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
        base.FixedUpdate();
        DetectObjects();
    }

    void DetectObjects()
    {
        // 現在の位置を中心にボックス範囲を設定
        detectionCenter = transform.position;
        // 指定したサイズとレイヤーで範囲内のすべてのオブジェクトを検出
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(detectionCenter, boxSize, 0f, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            Character detectedObject = hitCollider.GetComponent<Character>();

            if (detectedObject != null && !enemies.Contains(detectedObject))
            {
                // 敵リストに追加
                // 検知した敵キャラクターを攻撃対象に設定
                enemyCharacter = detectedObject;
                enemies.Add(detectedObject);
                Debug.Log("検知したオブジェクト: " + hitCollider.name);
            }
        }

        // 検知した敵キャラクターを攻撃対象に設定
        if(enemies.Count < 0) { return; }
        SetNextEnemy();
        if (enemyCharacter == null || IsDead) { return; }
        if (characterState == CharacterState.Attack) { return; }
        //　攻撃処理？

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
