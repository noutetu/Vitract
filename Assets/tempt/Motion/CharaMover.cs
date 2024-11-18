using UnityEngine;

public class CharaMover : MonoBehaviour
{
    private Rigidbody2D rb;  // Rigidbody2D を使用する場合
    Vector2 direction;  // デフォルトは左方向

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();  // Rigidbody2Dの参照を取得
    }

    public void Move(float speed, bool isPlayer)
    {
        direction = isPlayer ? Vector2.right : Vector2.left;
        // Rigidbodyを使った移動
        rb.MovePosition(rb.position + (direction * speed * Time.deltaTime * GameManager.Instance.gameSpeed));
    }

}
