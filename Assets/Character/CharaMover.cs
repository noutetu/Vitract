using UnityEngine;

public class CharaMover : MonoBehaviour
{
    Vector3 direction = Vector3.left;  // デフォルトは左方向

    public void Move(float speed, bool isPlayer)
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
