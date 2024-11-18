using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TargetList : MonoBehaviour
{
    public ReactiveCollection<IDamageable> enemies;  // 攻撃対象の敵リスト
    IDamageable nemy;

    public TargetList()
    {
        enemies = new ReactiveCollection<IDamageable>();
            
    }
// ------------------- リストから削除 -------------------
    public void RemoveInEnemies(IDamageable targetEnemy)
    {
        if (enemies.Contains(targetEnemy))
        {
            enemies.Remove(targetEnemy);
        }
    }
// ------------------- リストに追加 -------------------
    public void RegisterAtEnemies(IDamageable targetEnemy)
    {
        Character character = targetEnemy as Character;

        // ターゲットがすでに死んでいたらreturn
        if (targetEnemy != null)
        {
            // HPを購読し、0以下になったときにリストから削除する
            if (character.currentHp.Value <= 0|| character == null) { return; }
            
        }

        // enemiesリストにまだ登録されていない場合のみ登録する
        if (!enemies.Contains(targetEnemy))
        {
            enemies.Add(targetEnemy);
            character.currentHp
                .Where(hp => hp <= 0)
                .Subscribe(_ =>
                {
                    RemoveInEnemies(targetEnemy);
                })
                .AddTo(this); // このコンポーネントが破棄されたら購読も解除
        }
    }
// ------------------- リストに追加 -------------------
public IDamageable SetNextEnemy()
{
    return enemies[0];
}    
}
