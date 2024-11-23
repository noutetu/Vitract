using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UIElements;

[Serializable]
public class TargetList : MonoBehaviour
{
    public ReactiveCollection<IDamageable> enemies;  // 攻撃対象の敵リスト
    private CompositeDisposable disposables;

    public void Initialize()
    {
        enemies = new ReactiveCollection<IDamageable>();
        disposables = new CompositeDisposable();
    }
    
    // ------------------- 敵リストから削除 -------------------
    public void RemoveInEnemies(IDamageable targetEnemy)
    {
        if (targetEnemy != null && enemies.Contains(targetEnemy))
        {
            enemies.Remove(targetEnemy);
        }
    }

    // ------------------- 敵リストに追加 -------------------
    public void RegisterAtEnemies(IDamageable targetEnemy)
    {
        if (targetEnemy == null || targetEnemy.currentHp == null)
        {
            return; // ターゲットが無効であればリストに追加しない
        }

        // HPが0以下の場合はリストに登録しない
        if (targetEnemy.currentHp.Value <= 0) { return; }

        // enemiesリストにまだ登録されていない場合のみ登録する
        if (!enemies.Contains(targetEnemy))
        {
            enemies.Add(targetEnemy);
        }
    }

    // ------------------- 次の敵を設定 -------------------
    public IDamageable SetNextEnemy()
    {
        if (enemies.Count > 0)
        {
            return enemies[0];
        }
        return null;
    }

    public bool CheckContainsEnemy(IDamageable targetEnemy)
    {
        return enemies.Contains(targetEnemy);
    }

    // ------------------- クリーンアップ -------------------
    private void OnDestroy()
    {
        disposables.Dispose(); // すべての購読を解除
    }
}

