using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[Serializable]
public class TargetList : MonoBehaviour
{
    public ReactiveCollection<IDamageable> enemies;  // 攻撃対象の敵リスト
    private CompositeDisposable disposables = new CompositeDisposable(); // 購読の管理用

    public void Initialize()
    {
        enemies = new ReactiveCollection<IDamageable>();
    }

    public void set()
    {
        
    }

    // ------------------- リストから削除 -------------------
    public void RemoveInEnemies(IDamageable targetEnemy)
    {
        if (targetEnemy != null && enemies.Contains(targetEnemy))
        {
            enemies.Remove(targetEnemy);
        }
    }

    // ------------------- リストに追加 -------------------
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

            // HPが0以下になったときにリストから削除する購読を追加
            targetEnemy.currentHp
                .Skip(1) // 初期値をスキップして、変化があった時のみ反応
                .Where(hp => hp <= 0)
                .Subscribe(_ =>
                {
                    RemoveInEnemies(targetEnemy);
                })
                .AddTo(disposables); // 購読を管理リストに追加
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

    // ------------------- クリーンアップ -------------------
    private void OnDestroy()
    {
        disposables.Dispose(); // すべての購読を解除
    }

    
}


