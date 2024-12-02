using System;
using UnityEngine;

// 特殊攻撃のスキルクラス
public class JumpAttack : Skill
{
    // 派生クラス特有の初期化
    public override void Awake()
    {
        base.Awake();

        // スキル効果を設定
        SkillEffect = (attacker, target) =>
        {
            float damage = SkillValue; // SkillBaseからダメージ値を取得
            if (target != null)
            {
                target.TakeDamage(damage); // Targetにダメージを与える
                Debug.Log($"{SkillName}が{target}に{damage}のダメージを与えました。");
            }
        };
    }
}