using UnityEngine;

public class CharaMover : MonoBehaviour
{
    private bool isMoving = true;
    Vector3 direction = Vector3.left;  // デフォルトは左方向

    public void Move(float speed, bool isPlayer)
    {
        if(!isMoving) {return;}
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void Stop()
    {
        isMoving = false;
    }
}
