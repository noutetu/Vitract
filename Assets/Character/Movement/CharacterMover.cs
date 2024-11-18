using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Vitract.Character.Movement
{
    public class CharacterMover : MonoBehaviour
    {
        private Rigidbody2D rb;
        private bool canWalk;
        Vector2 direction; // デフォルトは左方向


        public void Awake()
        {
            canWalk = true;
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("rb is null");
            }
        }
        public void Move(float speed, bool isPlayer)
        {
            if (!canWalk) { Debug.Log("歩けない"); return; }
            direction = isPlayer ? Vector2.right : Vector2.left;
            // Rigidbodyを使った移動
            rb.MovePosition(rb.position + (direction * speed * Time.deltaTime));
        }

        public void Stop()
        {
            canWalk = false;
        }
    }
}
