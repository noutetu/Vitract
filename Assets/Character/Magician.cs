using UnityEngine;

public class Magician : Character
{
    public LayerMask hitLayers;  // レイキャストを当てるレイヤーを指定

    private void Update()
    {
        PerformRaycastAttack();
    }

    // レイキャストによる遠距離攻撃
    private void PerformRaycastAttack()
    {
        Vector2 rayOrigin = transform.position;   // レイキャストの始点
        Vector2 rayDirection = -transform.right;  // 左方向にレイキャストを飛ばす

        // レイキャストで全てのヒットを取得 (味方を無視するため)
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, rayDirection, range, hitLayers);

        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                GameObject hitObject = hit.collider.gameObject;

                // 自身と同じタグを持つオブジェクト（味方）なら無視する
                if (IsOwnBase(hitObject.tag))
                {
                    Debug.Log("Ignored friendly unit: " + hitObject.name);
                    continue; // 次のオブジェクトをチェック
                }
                else
                {
                    // 敵にヒットした場合
                    Debug.Log("Hit enemy: " + hitObject.name);
                    
                    // ダメージを与えるなどの処理を実行
                    DealDamageToEnemy(hitObject);

                    // レイキャストを終了
                    Debug.Log("Raycast stopped after hitting enemy.");
                    return; // 敵に当たったのでレイキャストを終了する
                }
            }
        }
    }

    // 敵にダメージを与える関数（仮の実装）
    private void DealDamageToEnemy(GameObject enemy)
    {
        // ここで敵にダメージを与える処理を実装します
        // 例: enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        Debug.Log("Dealt damage to: " + enemy.name);
    }

    // デバッグ用にレイを可視化（シーンビューで確認）
    private void OnDrawGizmosSelected()
{
    Vector2 rayOrigin = transform.position;
    Vector2 rayDirection = -transform.right;  // 左方向にレイを飛ばす

    Gizmos.color = Color.red;

    // レイの終点を計算して可視化
    Vector2 rayEnd = rayOrigin + rayDirection * range;
    Gizmos.DrawLine(rayOrigin, rayEnd);  // レイの始点から終点までをラインで描画
}

}
