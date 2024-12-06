using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange
{
    private Action<IDamageable> action;

    public AttackRange(Action<IDamageable> addToList)
    {
        action = addToList;
    }
    public void DetectEnemy(Vector2 center, Vector2 boxSize, LayerMask targetLayer)
    {
        // 指定したサイズとレイヤーで範囲内のすべてのオブジェクトを検出
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(center, boxSize, 0f, targetLayer);

        foreach (var hitCollider in hitColliders)
        {
            // IDamageable をキャッシュして使う
            if (hitCollider.TryGetComponent(out IDamageable enemy))
            {
                action(enemy);
            }
            Debug.Log("検知したオブジェクト: " + hitCollider.name);
        }

        // デバッグ用に範囲を可視化
        Debug.DrawRay(center, Vector2.right * boxSize.x / 2, Color.red);
        Debug.DrawRay(center, Vector2.up * boxSize.y / 2, Color.red);
    }
}
