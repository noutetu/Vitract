using UnityEngine;

public class CharaMover : MonoBehaviour
{
    public void Move(float speed)
    {
        // 横方向に移動させる
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
